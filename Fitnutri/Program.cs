using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Amazon.SimpleEmailV2;
using Amazon;
using Fitnutri.Application.Email;
using Fitnutri.Application.Services;
using Fitnutri.Auth;
using Fitnutri.Contracts;
using Fitnutri.Domain;
using Fitnutri.Infrastructure;
using Fitnutri.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Util: carrega RSA a partir do PEM
RSA LoadRsaFromPem(string pem)
{
    var rsa = RSA.Create();
    rsa.ImportFromPem(pem.AsSpan());
    return rsa;
}

// ===== Serilog =====
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

// ===== EF Core =====
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var conn = builder.Configuration.GetConnectionString("Sql");
    opt.UseSqlServer(conn, sql => sql.EnableRetryOnFailure());
});

// ===== Options =====
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection("ApiKey"));

// ===== JWT + Auth =====
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetBytes(jwt.Key).Length < 32)
    throw new InvalidOperationException("Jwt:Key deve ter pelo menos 32 bytes (256 bits).");

// Config de cookie via appsettings (opcional)
var cookieName = builder.Configuration["AuthCookie:Name"] ?? "fitnutri_auth";
var cookieDomain = builder.Configuration["AuthCookie:Domain"] ?? ".fit-nutri.com"; // <-- ajuste para seu domínio
var cookieHours = int.TryParse(builder.Configuration["AuthCookie:Hours"], out var h) ? h : 8;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // manter "sub" e "role"
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "role"
        };

        // Lê o token do COOKIE HttpOnly
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(cookieName, out var tokenFromCookie) &&
                    !string.IsNullOrWhiteSpace(tokenFromCookie))
                {
                    context.Token = tokenFromCookie;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// ===== CORS (Blazor Web App em app.fit-nutri.com chamando api.fit-nutri.com) =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("app", p =>
        p.WithOrigins(
            "https://fit-nutri.com",
            "https://api.fit-nutri.com",
            "capacitor://localhost",    // Capacitor apps
            "ionic://localhost",         // Ionic apps  
            "http://localhost",          // Local WebView
            "https://localhost",         // Local WebView HTTPS
            "file://"                    // WebView local files
        )
         .SetIsOriginAllowed(origin => true) // Permite WebView de apps mobile
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// ===== DI =====
builder.Services.AddScoped<IAuthService, AuthService>();

// ===== HTTP Client e ViaCEP Service =====
builder.Services.AddHttpClient<IViaCepService, ViaCepService>(client =>
{
    client.BaseAddress = new Uri("https://viacep.com.br/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// ===== Controllers =====
builder.Services.AddControllers();

// ===== SignalR para videochamada =====
builder.Services.AddSignalR();

// ===== Swagger + segurança (Bearer + x-api-key) =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fitnutri API", Version = "v1" });

    var bearer = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Bearer token"
    };
    c.AddSecurityDefinition("Bearer", bearer);

    var apiKey = new OpenApiSecurityScheme
    {
        Name = builder.Configuration.GetValue<string>("ApiKey:Header") ?? "x-api-key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Chave de API"
    };
    c.AddSecurityDefinition("ApiKey", apiKey);

    // Aplica os requisitos em todas as operações
    c.OperationFilter<GlobalSecurityRequirementsOperationFilter>();
});

// ===== Rate Limiter =====
builder.Services.AddRateLimiter(options =>
{
    // Registro por IP
    options.AddPolicy("register-ip", httpContext =>
    {
        var key = GetClientIp(httpContext) ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10, // 10 registros/min/IP
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    // Login por IP (anti brute force)
    options.AddPolicy("login-ip", httpContext =>
    {
        var key = GetClientIp(httpContext) ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30, // 30 tentativas/min/IP
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    // Esqueci senha por IP - NOVO RATE LIMIT
    options.AddPolicy("forgot-password-ip", httpContext =>
    {
        var key = GetClientIp(httpContext) ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5, // 5 tentativas/min/IP
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});

static string? GetClientIp(HttpContext ctx)
{
    if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var xff) && !string.IsNullOrWhiteSpace(xff))
        return xff.ToString().Split(',')[0].Trim();
    return ctx.Connection.RemoteIpAddress?.ToString();
}

// ===== AWS SES =====
builder.Services.AddSingleton<IAmazonSimpleEmailServiceV2>(_ =>
{
    var region = builder.Configuration["AWS:Region"] ?? "us-east-1";
    return new AmazonSimpleEmailServiceV2Client(RegionEndpoint.GetBySystemName(region));
});

// ===== Email Sender =====
builder.Services.AddScoped<IEmailSender, AwsSesEmailSender>();

var app = builder.Build();

// ===== Middlewares =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("app");

app.UseRateLimiter();

// x-api-key para TODAS as rotas (inclui /auth/*). Se quiser liberar /auth/*, mova o middleware para um MapGroup.
app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// ===== Controllers =====
app.MapControllers();

// ===== SignalR Hub =====
app.MapHub<Fitnutri.Application.VideoCallHub>("/videocall").RequireAuthorization();

// ===== Endpoints =====

// ---------- AUTH ----------
app.MapPost("/auth/register", async (IAuthService auth, RegisterRequest req, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "Dados inválidos." });

    try
    {
        var user = await auth.RegisterAsync(req.UserName, req.Email, req.Password, ct);
        return Results.Created($"/users/{user.Id}", new { message = "Usuário criado. Aguarde aprovação.", userId = user.Id });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
}).RequireRateLimiting("register-ip")
  .WithTags("Auth");

app.MapPost("/auth/login", async (HttpContext http, IAuthService auth, LoginRequest req, CancellationToken ct) =>
{
    try
    {
        var (user, token, exp) = await auth.LoginAsync(req.UserNameOrEmail.Trim(), req.Password, ct);

        // Escreve COOKIE HttpOnly com o JWT
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,                  // Produção: exige HTTPS
            SameSite = SameSiteMode.None,   // Para funcionar entre domínios (app.* -> api.*)
            Expires = exp,                  // Usa a expiração do token
            Path = "/",
            Domain = cookieDomain           // ".fit-nutri.com" para subdomínios
        };
        http.Response.Cookies.Append(cookieName, token, cookieOptions);

        // Retorna também no corpo (compatibilidade com clientes mobile, se quiser)
        return Results.Ok(new AuthResponse(token, exp));
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Usuário não aprovado"))
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("E-mail não verificado"))
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("incorret"))
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch
    {
        return Results.BadRequest();
    }
}).RequireRateLimiting("login-ip")
  .WithTags("Auth");

app.MapPost("/auth/confirm-email-by-identifier", async (ConfirmEmailByIdentifierRequest req, AppDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.EmailOrUsername))
        return Results.BadRequest(new { error = "Email ou nome de usuário é obrigatório." });

    // Buscar usuário por email ou username
    var user = await db.Users.FirstOrDefaultAsync(x =>
        x.Email == req.EmailOrUsername.Trim() ||
        x.UserName == req.EmailOrUsername.Trim(), ct);

    if (user is null)
        return Results.NotFound(new { error = "Usuário não encontrado." });

    if (user.EmailConfirmed)
        return Results.Ok(new { message = "E-mail já confirmado." });

    if (user.EmailVerificationCode is null)
        return Results.BadRequest(new { error = "Não há código pendente para este usuário." });

    if (user.EmailVerificationCode != req.Code)
        return Results.BadRequest(new { error = "Código inválido." });

    user.EmailConfirmed = true;
    user.EmailVerificationCode = null; // limpa após confirmar
    await db.SaveChangesAsync(ct);

    return Results.Ok(new { message = "E-mail confirmado com sucesso." });
}).WithTags("Auth");

// Logout: limpa o cookie
app.MapPost("/auth/logout", (HttpContext http) =>
{
    http.Response.Cookies.Delete(cookieName, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = "/",
        Domain = cookieDomain
    });
    return Results.Ok(new { message = "Logout efetuado." });
}).WithTags("Auth");

// Confirmação de e-mail
app.MapPost("/auth/confirm-email", async (ConfirmEmailRequest req, AppDbContext db, CancellationToken ct) =>
{
    var user = await db.Users.FirstOrDefaultAsync(x => x.Id == req.UserId, ct);
    if (user is null) return Results.NotFound(new { error = "Usuário não encontrado." });

    if (user.EmailConfirmed)
        return Results.Ok(new { message = "E-mail já confirmado." });

    if (user.EmailVerificationCode is null)
        return Results.BadRequest(new { error = "Não há código pendente para este usuário." });

    if (user.EmailVerificationCode != req.Code)
        return Results.BadRequest(new { error = "Código inválido." });

    user.EmailConfirmed = true;
    user.EmailVerificationCode = null; // limpa após confirmar
    await db.SaveChangesAsync(ct);

    return Results.Ok(new { message = "E-mail confirmado com sucesso." });
}).WithTags("Auth");

// NOVOS ENDPOINTS DE ESQUECI SENHA
app.MapPost("/auth/forgot-password", async (IAuthService auth, ForgotPasswordRequest req, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.Email))
        return Results.BadRequest(new { error = "E-mail é obrigatório." });

    try
    {
        var message = await auth.ForgotPasswordAsync(req.Email, ct);
        return Results.Ok(new ForgotPasswordResponse(message));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireRateLimiting("forgot-password-ip")
  .WithTags("Auth");

app.MapPost("/auth/reset-password", async (IAuthService auth, ResetPasswordRequest req, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.Token) || string.IsNullOrWhiteSpace(req.NewPassword))
        return Results.BadRequest(new { error = "Token e nova senha são obrigatórios." });

    try
    {
        await auth.ResetPasswordAsync(req.Token, req.NewPassword, ct);
        return Results.Ok(new { message = "Senha redefinida com sucesso." });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).WithTags("Auth");

// Quem sou eu (autenticado via cookie/JWT)
app.MapGet("/users/me", async (AppDbContext db, HttpContext ctx, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();

    var id = Guid.Parse(sub);
    var user = await db.Users.FindAsync([id], ct);
    if (user is null) return Results.NotFound();

    return Results.Ok(new MeResponse(user.Id, user.UserName, user.Email, user.CreatedAt, user.EmailConfirmed, user.Status));
})
.RequireAuthorization()
.WithTags("Users");

// ---------- ADMIN ----------
var adminGroup = app.MapGroup("/admin").WithTags("Admin");
adminGroup.RequireAuthorization("AdminOnly");

// Listar usuários pendentes
adminGroup.MapGet("/users/pending", async (AppDbContext db, int skip = 0, int take = 20, CancellationToken ct = default) =>
{
    take = Math.Clamp(take, 1, 100);
    var users = await db.Users
        .Where(u => u.Status == UserStatus.Pending)
        .OrderBy(u => u.CreatedAt)
        .Skip(skip).Take(take)
        .Select(u => new
        {
            u.Id,
            u.UserName,
            u.Email,
            u.CreatedAt,
            u.EmailConfirmed,
            u.Status
        })
        .ToListAsync(ct);

    return Results.Ok(users);
});
//Usuarios Aprovados
adminGroup.MapGet("/users/approved", async (AppDbContext db, int skip = 0, int take = 20, CancellationToken ct = default) =>
{
    take = Math.Clamp(take, 1, 100);
    var users = await db.Users
        .Where(u => u.Status == UserStatus.Approved)
        .OrderBy(u => u.CreatedAt)
        .Skip(skip).Take(take)
        .Select(u => new
        {
            u.Id,
            u.UserName,
            u.Email,
            u.CreatedAt,
            u.EmailConfirmed,
            u.Status
        })
        .ToListAsync(ct);

    return Results.Ok(users);
});
//Usuarios Rejeitados
adminGroup.MapGet("/users/rejects", async (AppDbContext db, int skip = 0, int take = 20, CancellationToken ct = default) =>
{
    take = Math.Clamp(take, 1, 100);
    var users = await db.Users
        .Where(u => u.Status == UserStatus.Rejected)
        .OrderBy(u => u.CreatedAt)
        .Skip(skip).Take(take)
        .Select(u => new
        {
            u.Id,
            u.UserName,
            u.Email,
            u.CreatedAt,
            u.EmailConfirmed,
            u.Status
        })
        .ToListAsync(ct);

    return Results.Ok(users);
});
// Excluir usuário
adminGroup.MapDelete("/users/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var user = await db.Users.FindAsync([id], ct);
    if (user is null) return Results.NotFound();
    db.Users.Remove(user);
    await db.SaveChangesAsync(ct);
    return Results.Ok(new { message = "Usuário excluído.", user.Id, user.UserName, user.Email });
});

// Aprovar usuário + enviar e-mail com código
adminGroup.MapPost("/users/{id:guid}/approve",
    async (Guid id, ApproveUserRequest req, AppDbContext db, IEmailSender emailSender, IConfiguration cfg, ILoggerFactory lf, CancellationToken ct) =>
    {
        try
        {
            var log = lf.CreateLogger("ApproveUser");
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (user is null) return Results.NotFound();

            if (user.Status == UserStatus.Approved)
                return Results.BadRequest(new { error = "Usuário já está aprovado." });

            user.Status = UserStatus.Approved;
            user.ApprovedAt = DateTime.UtcNow;
            user.ApprovedBy = string.IsNullOrWhiteSpace(req?.ApprovedBy) ? "admin" : req!.ApprovedBy;
            user.EmailConfirmed = false; // força confirmar e-mail
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000);
            user.EmailVerificationCode = code;

            var codeStr = code.ToString("D6");
            var subject = "Confirme seu e-mail - Código de verificação";
            var html = $"""
                <p>Olá {user.UserName},</p>
                <p>Seu cadastro foi aprovado. Para confirmar seu e-mail, use o código abaixo no primeiro login:</p>
                <h2 style="letter-spacing:3px;margin:16px 0;">{codeStr}</h2>
                <p>Se não foi você, ignore esta mensagem.</p>
                """;

            try
            {
                await emailSender.SendAsync(user.Email, subject, html, ct);
                log.LogInformation("Código de verificação enviado para {Email}", user.Email);
                await db.SaveChangesAsync(ct);
            }
            catch (Amazon.SimpleEmailV2.Model.MessageRejectedException ex)
            {
                log.LogError(ex, "SES rejeitou a mensagem para {Email}", user.Email);
                return Results.Problem("Falha ao enviar e-mail. Verifique domínio/remetente no SES.", statusCode: 502);
            }

            return Results.Ok(new
            {
                message = "Usuário aprovado. Código de verificação enviado por e-mail.",
                user.Id,
                user.Status,
                user.ApprovedAt,
                user.ApprovedBy
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 400);
        }
    });

// Confirmar e-mail do usuário (forçar)
adminGroup.MapPut("/users/{id:guid}/confimed-email",
    async (Guid id,  AppDbContext db, IEmailSender emailSender, IConfiguration cfg, ILoggerFactory lf, CancellationToken ct) =>
    {
        try
        {
            var log = lf.CreateLogger("ConfimedEmailUser");
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (user is null) return Results.NotFound();

            if (user.Status == UserStatus.Pending || user.Status == UserStatus.Rejected)
                return Results.BadRequest(new { error = "Usuário não está aprovado" });
            
            if (user.EmailConfirmed)
                return Results.BadRequest(new { error = "Usuário já está com email confirmado" });

            user.EmailConfirmed = true; // força confirmar e-mail
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (Amazon.SimpleEmailV2.Model.MessageRejectedException ex)
            {
            }

            return Results.Ok(new
            {
                message = "Email confirmado com sucesso",
                user.Id,
                user.Status,
                user.ApprovedAt,
                user.ApprovedBy,
                user.EmailConfirmed
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 400);
        }
    });

// Rejeitar usuário
adminGroup.MapPost("/users/{id:guid}/reject", async (Guid id, RejectUserRequest req, AppDbContext db, CancellationToken ct) =>
{
    var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
    if (user is null) return Results.NotFound();

    if (user.Status == UserStatus.Rejected)
        return Results.BadRequest(new { error = "Usuário já está rejeitado." });

    user.Status = UserStatus.Rejected;
    user.ApprovedAt = DateTime.UtcNow;
    user.ApprovedBy = string.IsNullOrWhiteSpace(req?.ApprovedBy) ? "admin" : req!.ApprovedBy;

    await db.SaveChangesAsync(ct);
    return Results.Ok(new { message = "Usuário rejeitado.", user.Id, user.Status, user.ApprovedAt, user.ApprovedBy, req?.Reason });
});

// ---------- PERFIS (admin only) ----------
var perfisGroup = app.MapGroup("/perfis").RequireAuthorization("AdminOnly");

perfisGroup.MapGet("/", async (AppDbContext db) =>
    await db.Perfis.ToListAsync()
);

perfisGroup.MapGet("/{id}", async (AppDbContext db, Guid id) =>
    await db.Perfis.FindAsync(id) is Perfil perfil ? Results.Ok(perfil) : Results.NotFound()
);

perfisGroup.MapPost("/", async (AppDbContext db, Perfil perfil) =>
{
    perfil.Id = Guid.NewGuid();
    db.Perfis.Add(perfil);
    await db.SaveChangesAsync();
    return Results.Created($"/api/perfis/{perfil.Id}", perfil);
});

perfisGroup.MapPut("/{id}", async (AppDbContext db, Guid id, Perfil perfil) =>
{
    if (id != perfil.Id) return Results.BadRequest();
    var exists = await db.Perfis.AnyAsync(p => p.Id == id);
    if (!exists) return Results.NotFound();
    db.Entry(perfil).State = EntityState.Modified;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

perfisGroup.MapDelete("/{id}", async (AppDbContext db, Guid id) =>
{
    var perfil = await db.Perfis.FindAsync(id);
    if (perfil is null) return Results.NotFound();
    db.Perfis.Remove(perfil);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ---------- PERFIS USUÁRIO (autenticado) ----------
var userPerfilGroup = app.MapGroup("/user/perfil").RequireAuthorization().WithTags("UserPerfil");
// Associar perfil ao usuário
userPerfilGroup.MapPost("/associar", async (AssociarPerfilRequest req, AppDbContext db, HttpContext ctx, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();

    var userId = Guid.Parse(sub);
    var user = await db.Users.Include(u => u.Perfil).FirstOrDefaultAsync(u => u.Id == userId, ct);
    
    if (user is null)
        return Results.NotFound(new { error = "Usuário não encontrado" });

    // Verificar se já tem perfil
    if (user.Perfil != null)
        return Results.BadRequest(new { error = "Usuário já possui um perfil" });

    // Buscar ou criar o perfil pelo tipo
    var perfil = await db.Perfis.FirstOrDefaultAsync(p => p.Tipo == req.TipoPerfil, ct);
    if (perfil == null)
    {
        // Criar o perfil se não existir
        perfil = new Perfil
        {
            Id = Guid.NewGuid(),
            Tipo = req.TipoPerfil,
            Nome = req.TipoPerfil switch
            {
                PerfilTipo.Nutricionista => "Nutricionista",
                PerfilTipo.PersonalTrainer => "Personal Trainer",
                PerfilTipo.Paciente => "Paciente",
                _ => "Perfil"
            }
        };
        db.Perfis.Add(perfil);
    }

    // Associar o perfil ao usuário
    user.PerfilId = perfil.Id;
    user.Perfil = perfil;
    await db.SaveChangesAsync(ct);

    return Results.Ok(new { 
        message = "Perfil associado com sucesso",
        perfil = new { 
            id = perfil.Id,
            tipo = perfil.Tipo,
            nome = perfil.Nome
        }
    });
});

// Obter perfis do usuário
userPerfilGroup.MapGet("/meus-perfis", async (AppDbContext db, HttpContext ctx, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();

    var userId = Guid.Parse(sub);
    var user = await db.Users.Include(u => u.Perfil).FirstOrDefaultAsync(u => u.Id == userId, ct);
    
    if (user is null)
        return Results.NotFound(new { error = "Usuário não encontrado" });

    var perfis = user.Perfil != null ? new[]
    {
        new
        {
            id = user.Perfil.Id,
            tipo = user.Perfil.Tipo,
            nome = user.Perfil.Nome
        }
    } : Array.Empty<object>();

    return Results.Ok(perfis);
});

// Obter tipos de perfil disponíveis
userPerfilGroup.MapGet("/tipos-disponiveis", () =>
{
    var tiposDisponiveis = new[]
    {
        new { id = 2, nome = "Nutricionista", descricao = "Profissional da área que quer atender pacientes" },
        new { id = 3, nome = "Personal Trainer", descricao = "Educador físico que quer acompanhar alunos" },
        new { id = 4, nome = "Paciente", descricao = "Busca orientação nutricional e acompanhamento" }
    };

    return Results.Ok(tiposDisponiveis);
});

// Remover perfil do usuário
userPerfilGroup.MapDelete("/remover/{perfilId:guid}", async (Guid perfilId, AppDbContext db, HttpContext ctx, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();

    var userId = Guid.Parse(sub);
    var user = await db.Users.Include(u => u.Perfil).FirstOrDefaultAsync(u => u.Id == userId, ct);
    
    if (user is null)
        return Results.NotFound(new { error = "Usuário não encontrado" });

    var perfil = user.Perfil;
    if (perfil == null || perfil.Id != perfilId)
        return Results.NotFound(new { error = "Perfil não encontrado ou não pertence ao usuário" });

    user.Perfil = null;
    user.PerfilId = null;
    await db.SaveChangesAsync(ct);

    return Results.Ok(new { message = "Perfil removido com sucesso" });
});


// ---------- AGENDAMENTOS ----------
var agGroup = app.MapGroup("/agendamentos").WithTags("Agendamentos");

// Disponibilidade de horários por profissional e data (intervalo de 1h entre 09:00 e 17:00)
agGroup.MapGet("/disponibilidade", async (Guid profissionalId, DateOnly data, AppDbContext db, CancellationToken ct) =>
{
    var horarios = new List<string>();
    for (int h = 9; h <= 17; h++)
    {
        var slot = new TimeOnly(h, 0);
        var ocupado = await db.Agendamentos
            .AnyAsync(a => a.ProfissionalId == profissionalId && a.Data == data && a.Hora == slot && a.Status != AgendamentoStatus.Cancelado, ct);
        if (!ocupado)
            horarios.Add(slot.ToString("HH:mm"));
    }
    return Results.Ok(new Fitnutri.Contracts.DisponibilidadeResponse(horarios));
}).RequireAuthorization();

// Criar agendamento
agGroup.MapPost("/", async (HttpContext ctx, AppDbContext db, Fitnutri.Contracts.CriarAgendamentoRequest req, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var clienteId = Guid.Parse(sub);

    // Impede duplo agendamento no mesmo slot
    var exists = await db.Agendamentos.AnyAsync(a => a.ProfissionalId == req.ProfissionalId && a.Data == req.Data && a.Hora == req.Hora && a.Status != AgendamentoStatus.Cancelado, ct);
    if (exists)
        return Results.Conflict(new { error = "Horário indisponível." });

    var novo = new Fitnutri.Domain.Agendamento
    {
        Id = Guid.NewGuid(),
        ProfissionalId = req.ProfissionalId,
        ClienteUserId = clienteId,
        Data = req.Data,
        Hora = req.Hora,
        DuracaoMinutos = 60,
        Status = AgendamentoStatus.Pendente,
        CreatedAt = DateTime.UtcNow
    };

    db.Agendamentos.Add(novo);
    await db.SaveChangesAsync(ct);

    var resp = new Fitnutri.Contracts.AgendamentoResponse(novo.Id, novo.ProfissionalId, novo.ClienteUserId, novo.Data, novo.Hora, novo.DuracaoMinutos, novo.Status);
    return Results.Created($"/agendamentos/{novo.Id}", resp);
}).RequireAuthorization();

// Listar agendamentos do usuário autenticado (cliente)
agGroup.MapGet("/me", async (HttpContext ctx, AppDbContext db, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var clienteId = Guid.Parse(sub);

    var agendamentos = await db.Agendamentos
        .Where(a => a.ClienteUserId == clienteId)
        .OrderByDescending(a => a.Data).ThenByDescending(a => a.Hora)
        .ToListAsync(ct);

    var profissionalIds = agendamentos.Select(a => a.ProfissionalId).Distinct().ToList();
    var profissionais = await db.Users
        .Where(u => profissionalIds.Contains(u.Id))
        .Include(u => u.Profile)
        .Include(u => u.Perfil)
        .ToListAsync(ct);

    var items = agendamentos.Select(a =>
    {
        var prof = profissionais.FirstOrDefault(p => p.Id == a.ProfissionalId);
        return new AgendamentoResponse(
            a.Id,
            a.ProfissionalId,
            a.ClienteUserId,
            a.Data,
            a.Hora,
            a.DuracaoMinutos,
            a.Status,
            prof?.Profile?.NomeCompleto,
            prof?.Perfil?.Nome
        );
    }).ToList();

    return Results.Ok(items);
}).RequireAuthorization();

// Listar agendamentos do profissional autenticado
agGroup.MapGet("/profissional/me", async (HttpContext ctx, AppDbContext db, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var profissionalId = Guid.Parse(sub);

    var agendamentos = await db.Agendamentos
        .Where(a => a.ProfissionalId == profissionalId)
        .OrderBy(a => a.Data).ThenBy(a => a.Hora)
        .ToListAsync(ct);

    var clienteIds = agendamentos.Select(a => a.ClienteUserId).Distinct().ToList();
    var clientes = await db.Users
        .Where(u => clienteIds.Contains(u.Id))
        .Include(u => u.Profile)
        .ToListAsync(ct);

    var items = agendamentos.Select(a =>
    {
        var cliente = clientes.FirstOrDefault(c => c.Id == a.ClienteUserId);
        return new AgendamentoResponse(
            a.Id,
            a.ProfissionalId,
            a.ClienteUserId,
            a.Data,
            a.Hora,
            a.DuracaoMinutos,
            a.Status,
            cliente?.Profile?.NomeCompleto ?? cliente?.UserName,
            null // Não precisa retornar perfil do profissional neste caso
        );
    }).ToList();

    return Results.Ok(items);
}).RequireAuthorization();

// Obter agendamento por ID (somente cliente ou profissional envolvidos)
agGroup.MapGet("/{id:guid}", async (Guid id, HttpContext ctx, AppDbContext db, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var userId = Guid.Parse(sub);

    var a = await db.Agendamentos.FirstOrDefaultAsync(x => x.Id == id, ct);
    if (a is null) return Results.NotFound();

    if (a.ClienteUserId != userId && a.ProfissionalId != userId)
        return Results.Forbid();

    return Results.Ok(new Fitnutri.Contracts.AgendamentoResponse(a.Id, a.ProfissionalId, a.ClienteUserId, a.Data, a.Hora, a.DuracaoMinutos, a.Status));
}).RequireAuthorization();

// Confirmar agendamento - somente profissional
agGroup.MapPut("/{id:guid}/confirmar", async (Guid id, HttpContext ctx, AppDbContext db, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var userId = Guid.Parse(sub);

    var a = await db.Agendamentos.FirstOrDefaultAsync(x => x.Id == id, ct);
    if (a is null) return Results.NotFound();
    
    // Apenas o profissional pode confirmar
    if (a.ProfissionalId != userId)
        return Results.Forbid();

    // Verificar se já está cancelado
    if (a.Status == AgendamentoStatus.Cancelado)
        return Results.BadRequest(new { error = "Não é possível confirmar um agendamento cancelado." });

    // Verificar se já está confirmado
    if (a.Status == AgendamentoStatus.Confirmado)
        return Results.Ok(new { message = "Agendamento já está confirmado.", agendamento = new AgendamentoResponse(a.Id, a.ProfissionalId, a.ClienteUserId, a.Data, a.Hora, a.DuracaoMinutos, a.Status) });

    a.Status = AgendamentoStatus.Confirmado;
    await db.SaveChangesAsync(ct);
    
    return Results.Ok(new { 
        message = "Agendamento confirmado com sucesso!", 
        agendamento = new AgendamentoResponse(a.Id, a.ProfissionalId, a.ClienteUserId, a.Data, a.Hora, a.DuracaoMinutos, a.Status) 
    });
}).RequireAuthorization();

// Cancelar agendamento - cliente ou profissional
agGroup.MapPut("/{id:guid}/cancelar", async (Guid id, HttpContext ctx, AppDbContext db, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var userId = Guid.Parse(sub);

    var a = await db.Agendamentos.FirstOrDefaultAsync(x => x.Id == id, ct);
    if (a is null) return Results.NotFound();
    
    // Cliente ou profissional podem cancelar
    if (a.ClienteUserId != userId && a.ProfissionalId != userId)
        return Results.Forbid();

    // Verificar se já está cancelado
    if (a.Status == AgendamentoStatus.Cancelado)
        return Results.Ok(new { message = "Agendamento já está cancelado.", agendamento = new AgendamentoResponse(a.Id, a.ProfissionalId, a.ClienteUserId, a.Data, a.Hora, a.DuracaoMinutos, a.Status) });

    a.Status = AgendamentoStatus.Cancelado;
    await db.SaveChangesAsync(ct);
    
    return Results.Ok(new { 
        message = "Agendamento cancelado com sucesso!", 
        agendamento = new AgendamentoResponse(a.Id, a.ProfissionalId, a.ClienteUserId, a.Data, a.Hora, a.DuracaoMinutos, a.Status) 
    });
}).RequireAuthorization();

// Deletar agendamento (hard delete) - somente cliente ou profissional
agGroup.MapDelete("/{id:guid}", async (Guid id, HttpContext ctx, AppDbContext db, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var userId = Guid.Parse(sub);

    var a = await db.Agendamentos.FirstOrDefaultAsync(x => x.Id == id, ct);
    if (a is null) return Results.NotFound();
    if (a.ClienteUserId != userId && a.ProfissionalId != userId)
        return Results.Forbid();

    db.Agendamentos.Remove(a);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
}).RequireAuthorization();


// ---------- DIETAS ----------
var dietGroup = app.MapGroup("/dietas").WithTags("Dietas").RequireAuthorization();

// Criar nova dieta
dietGroup.MapPost("/", async (HttpContext ctx, AppDbContext db, CreateDietRequest req, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var userId = Guid.Parse(sub);

    // Validações
    if (string.IsNullOrWhiteSpace(req.Title))
        return Results.BadRequest(new { error = "Título é obrigatório" });

    if (string.IsNullOrWhiteSpace(req.Description))
        return Results.BadRequest(new { error = "Descrição é obrigatória" });

    if (req.DayMeals.Count != 7)
        return Results.BadRequest(new { error = "É necessário fornecer exatamente 7 dias de refeições" });

    var validDays = new[] { "SEG", "TER", "QUA", "QUI", "SEX", "SAB", "DOM" };
    var providedDays = req.DayMeals.Select(d => d.Day.ToUpper()).ToList();
    
    if (!validDays.All(day => providedDays.Contains(day)))
        return Results.BadRequest(new { error = "Todos os dias da semana devem ser fornecidos (SEG, TER, QUA, QUI, SEX, SAB, DOM)" });

    // Criar dieta
    var diet = new Diet
    {
        Id = Guid.NewGuid(),
        ProfissionalId = userId,
        Title = req.Title.Trim(),
        Description = req.Description.Trim(),
        Type = req.Type,
        CreatedAt = DateTime.UtcNow,
        DayMeals = req.DayMeals.Select(dm => new DietDayMeal
        {
            Id = Guid.NewGuid(),
            Day = dm.Day.ToUpper(),
            Color = dm.Color,
            Breakfast = dm.Breakfast.Trim(),
            MorningSnack = dm.MorningSnack.Trim(),
            Lunch = dm.Lunch.Trim(),
            AfternoonSnack = dm.AfternoonSnack.Trim(),
            Dinner = dm.Dinner.Trim()
        }).ToList()
    };

    db.Diets.Add(diet);
    await db.SaveChangesAsync(ct);

    var response = new DietResponse(
        diet.Id,
        diet.ProfissionalId,
        diet.Title,
        diet.Description,
        diet.Type,
        diet.CreatedAt,
        diet.UpdatedAt,
        diet.DayMeals.OrderBy(dm => GetDayOrder(dm.Day)).Select(dm => new DayMealResponse(
            dm.Id,
            dm.Day,
            dm.Color,
            new MealResponse(dm.Breakfast, dm.MorningSnack, dm.Lunch, dm.AfternoonSnack, dm.Dinner)
        )).ToList(),
        0
    );

    return Results.Created($"/dietas/{diet.Id}", response);
});

// Listar minhas dietas
dietGroup.MapGet("/", async (HttpContext ctx, AppDbContext db, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var userId = Guid.Parse(sub);

    var diets = await db.Diets
        .Where(d => d.ProfissionalId == userId)
        .Select(d => new 
        {
            d.Id,
            d.Title,
            d.Description,
            d.Type,
            ActivePatientsCount = d.PatientDiets.Count(pd => pd.IsActive),
            d.CreatedAt
        })
        .OrderByDescending(d => d.CreatedAt)
        .ToListAsync(ct);

    var response = diets.Select(d => new DietSummaryResponse(
        d.Id,
        d.Title,
        d.Description,
        d.Type,
        d.ActivePatientsCount,
        d.CreatedAt
    )).ToList();

    return Results.Ok(response);
});

// Obter detalhes de uma dieta
dietGroup.MapGet("/{id:guid}", async (Guid id, HttpContext ctx, AppDbContext db, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var userId = Guid.Parse(sub);

    var diet = await db.Diets
        .Include(d => d.DayMeals)
        .Include(d => d.PatientDiets)
        .FirstOrDefaultAsync(d => d.Id == id, ct);

    if (diet is null)
        return Results.NotFound(new { error = "Dieta não encontrada" });

    // Verificar se tem permissão (é o profissional que criou ou é paciente com dieta atribuída)
    var isProfissional = diet.ProfissionalId == userId;
    var isPaciente = await db.PatientDiets
        .AnyAsync(pd => pd.DietId == id && pd.PatientUserId == userId && pd.IsActive, ct);

    if (!isProfissional && !isPaciente)
        return Results.Forbid();

    var response = new DietResponse(
        diet.Id,
        diet.ProfissionalId,
        diet.Title,
        diet.Description,
        diet.Type,
        diet.CreatedAt,
        diet.UpdatedAt,
        diet.DayMeals.OrderBy(dm => GetDayOrder(dm.Day)).Select(dm => new DayMealResponse(
            dm.Id,
            dm.Day,
            dm.Color,
            new MealResponse(dm.Breakfast, dm.MorningSnack, dm.Lunch, dm.AfternoonSnack, dm.Dinner)
        )).ToList(),
        diet.PatientDiets.Count(pd => pd.IsActive)
    );

    return Results.Ok(response);
});

// Atualizar dieta
dietGroup.MapPut("/{id:guid}", async (Guid id, HttpContext ctx, AppDbContext db, UpdateDietRequest req, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var userId = Guid.Parse(sub);

    var diet = await db.Diets
        .Include(d => d.DayMeals)
        .FirstOrDefaultAsync(d => d.Id == id && d.ProfissionalId == userId, ct);

    if (diet is null)
        return Results.NotFound(new { error = "Dieta não encontrada" });

    // Atualizar campos básicos
    if (!string.IsNullOrWhiteSpace(req.Title))
        diet.Title = req.Title.Trim();

    if (!string.IsNullOrWhiteSpace(req.Description))
        diet.Description = req.Description.Trim();

    if (req.Type.HasValue)
        diet.Type = req.Type.Value;

    if (req.DayMeals != null && req.DayMeals.Count > 0)
    {
        if (req.DayMeals.Count != 7)
            return Results.BadRequest(new { error = "É necessário fornecer exatamente 7 dias de refeições" });

        var validDays = new[] { "SEG", "TER", "QUA", "QUI", "SEX", "SAB", "DOM" };
        var providedDays = req.DayMeals.Select(d => d.Day.ToUpper()).ToList();
        
        if (!validDays.All(day => providedDays.Contains(day)))
            return Results.BadRequest(new { error = "Todos os dias da semana devem ser fornecidos" });

        // Deletar todos os DayMeals existentes diretamente no banco sem tracking
        await db.DietDayMeals.Where(dm => dm.DietId == diet.Id).ExecuteDeleteAsync(ct);
        
        // Limpar a coleção em memória
        diet.DayMeals.Clear();
        
        // Adicionar todos os novos DayMeals
        foreach (var reqDayMeal in req.DayMeals)
        {
            var dayUpper = reqDayMeal.Day.ToUpper();
            
            var newDayMeal = new DietDayMeal
            {
                Id = Guid.NewGuid(),
                DietId = diet.Id,
                Day = dayUpper,
                Color = reqDayMeal.Color,
                Breakfast = reqDayMeal.Breakfast.Trim(),
                MorningSnack = reqDayMeal.MorningSnack.Trim(),
                Lunch = reqDayMeal.Lunch.Trim(),
                AfternoonSnack = reqDayMeal.AfternoonSnack.Trim(),
                Dinner = reqDayMeal.Dinner.Trim()
            };
            
            db.DietDayMeals.Add(newDayMeal);
        }
    }

    diet.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync(ct);

    var response = new DietResponse(
        diet.Id,
        diet.ProfissionalId,
        diet.Title,
        diet.Description,
        diet.Type,
        diet.CreatedAt,
        diet.UpdatedAt,
        diet.DayMeals.OrderBy(dm => GetDayOrder(dm.Day)).Select(dm => new DayMealResponse(
            dm.Id,
            dm.Day,
            dm.Color,
            new MealResponse(dm.Breakfast, dm.MorningSnack, dm.Lunch, dm.AfternoonSnack, dm.Dinner)
        )).ToList(),
        await db.PatientDiets.CountAsync(pd => pd.DietId == diet.Id && pd.IsActive, ct)
    );

    return Results.Ok(response);
});

// Deletar dieta
dietGroup.MapDelete("/{id:guid}", async (Guid id, HttpContext ctx, AppDbContext db, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var userId = Guid.Parse(sub);

    var diet = await db.Diets
        .Include(d => d.PatientDiets)
        .FirstOrDefaultAsync(d => d.Id == id && d.ProfissionalId == userId, ct);

    if (diet is null)
        return Results.NotFound(new { error = "Dieta não encontrada" });

    // Verificar se há pacientes usando esta dieta
    if (diet.PatientDiets.Any(pd => pd.IsActive))
        return Results.BadRequest(new { error = "Não é possível excluir uma dieta que está sendo usada por pacientes. Desative-a primeiro." });

    db.Diets.Remove(diet);
    await db.SaveChangesAsync(ct);

    return Results.NoContent();
});

// Atribuir dieta a um paciente
dietGroup.MapPost("/assign", async (HttpContext ctx, AppDbContext db, AssignDietToPatientRequest req, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var userId = Guid.Parse(sub);

    // Verificar se a dieta existe e pertence ao nutricionista
    var diet = await db.Diets
        .FirstOrDefaultAsync(d => d.Id == req.DietId && d.ProfissionalId == userId, ct);

    if (diet is null)
        return Results.NotFound(new { error = "Dieta não encontrada" });

    // Verificar se o paciente existe
    var patient = await db.Users
        .Include(u => u.Profile)
        .FirstOrDefaultAsync(u => u.Id == req.PatientUserId, ct);

    if (patient is null)
        return Results.NotFound(new { error = "Paciente não encontrado" });

    // Desativar dietas ativas anteriores do paciente
    var activeDiets = await db.PatientDiets
        .Where(pd => pd.PatientUserId == req.PatientUserId && pd.IsActive)
        .ToListAsync(ct);

    foreach (var activeDiet in activeDiets)
    {
        activeDiet.IsActive = false;
        activeDiet.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    // Criar nova atribuição
    var patientDiet = new PatientDiet
    {
        Id = Guid.NewGuid(),
        PatientUserId = req.PatientUserId,
        DietId = req.DietId,
        StartDate = req.StartDate,
        EndDate = req.EndDate,
        IsActive = true,
        AssignedAt = DateTime.UtcNow
    };

    db.PatientDiets.Add(patientDiet);
    await db.SaveChangesAsync(ct);

    var response = new PatientDietResponse(
        patientDiet.Id,
        patient.Id,
        patient.Profile?.NomeCompleto ?? patient.UserName,
        diet.Id,
        diet.Title,
        patientDiet.StartDate,
        patientDiet.EndDate,
        patientDiet.IsActive,
        patientDiet.AssignedAt
    );

    return Results.Ok(response);
});

// Listar pacientes com uma dieta específica
dietGroup.MapGet("/{dietId:guid}/patients", async (Guid dietId, HttpContext ctx, AppDbContext db, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var userId = Guid.Parse(sub);

    // Verificar se a dieta pertence ao nutricionista
    var diet = await db.Diets
        .FirstOrDefaultAsync(d => d.Id == dietId && d.ProfissionalId == userId, ct);

    if (diet is null)
        return Results.NotFound(new { error = "Dieta não encontrada" });

    var patients = await db.PatientDiets
        .Where(pd => pd.DietId == dietId)
        .Include(pd => pd.Diet)
        .Select(pd => new
        {
            pd.Id,
            pd.PatientUserId,
            pd.DietId,
            pd.StartDate,
            pd.EndDate,
            pd.IsActive,
            pd.AssignedAt,
            DietTitle = pd.Diet.Title,
            PatientName = db.Users
                .Where(u => u.Id == pd.PatientUserId)
                .Select(u => u.Profile != null ? u.Profile.NomeCompleto : u.UserName)
                .FirstOrDefault()
        })
        .OrderByDescending(pd => pd.AssignedAt)
        .ToListAsync(ct);

    var response = patients.Select(p => new PatientDietResponse(
        p.Id,
        p.PatientUserId,
        p.PatientName ?? "Desconhecido",
        p.DietId,
        p.DietTitle,
        p.StartDate,
        p.EndDate,
        p.IsActive,
        p.AssignedAt
    )).ToList();

    return Results.Ok(response);
});

// Obter dieta ativa do paciente logado
dietGroup.MapGet("/my-active-diet", async (HttpContext ctx, AppDbContext db, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var userId = Guid.Parse(sub);

    var patientDiet = await db.PatientDiets
        .Include(pd => pd.Diet)
            .ThenInclude(d => d.DayMeals)
        .Where(pd => pd.PatientUserId == userId && pd.IsActive)
        .OrderByDescending(pd => pd.AssignedAt)
        .FirstOrDefaultAsync(ct);

    if (patientDiet is null)
        return Results.NotFound(new { error = "Nenhuma dieta ativa encontrada" });

    var diet = patientDiet.Diet;
    var response = new DietResponse(
        diet.Id,
        diet.ProfissionalId,
        diet.Title,
        diet.Description,
        diet.Type,
        diet.CreatedAt,
        diet.UpdatedAt,
        diet.DayMeals.OrderBy(dm => GetDayOrder(dm.Day)).Select(dm => new DayMealResponse(
            dm.Id,
            dm.Day,
            dm.Color,
            new MealResponse(dm.Breakfast, dm.MorningSnack, dm.Lunch, dm.AfternoonSnack, dm.Dinner)
        )).ToList(),
        await db.PatientDiets.CountAsync(pd => pd.DietId == diet.Id && pd.IsActive, ct)
    );

    return Results.Ok(response);
});

// Desativar dieta de um paciente
dietGroup.MapPost("/{dietId:guid}/deactivate/{patientUserId:guid}", async (Guid dietId, Guid patientUserId, HttpContext ctx, AppDbContext db, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();
    var userId = Guid.Parse(sub);

    // Verificar se a dieta pertence ao nutricionista
    var diet = await db.Diets
        .FirstOrDefaultAsync(d => d.Id == dietId && d.ProfissionalId == userId, ct);

    if (diet is null)
        return Results.NotFound(new { error = "Dieta não encontrada" });

    var patientDiet = await db.PatientDiets
        .FirstOrDefaultAsync(pd => pd.DietId == dietId && pd.PatientUserId == patientUserId && pd.IsActive, ct);

    if (patientDiet is null)
        return Results.NotFound(new { error = "Atribuição de dieta não encontrada ou já inativa" });

    patientDiet.IsActive = false;
    patientDiet.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);

    await db.SaveChangesAsync(ct);

    return Results.NoContent();
});

// Helper function para ordenar dias da semana
static int GetDayOrder(string day)
{
    return day.ToUpper() switch
    {
        "SEG" => 1,
        "TER" => 2,
        "QUA" => 3,
        "QUI" => 4,
        "SEX" => 5,
        "SAB" => 6,
        "DOM" => 7,
        _ => 8
    };
}

// ---------- HEALTH ----------

var commonGroup = app.MapGroup("/");

commonGroup.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
commonGroup.MapGet("/readyz", async (AppDbContext db, CancellationToken ct) =>
{
    var ok = await db.Database.CanConnectAsync(ct);
    return ok ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503);
});

app.Run();
