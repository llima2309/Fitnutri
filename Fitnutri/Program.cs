using System.Text;
using System.Threading.RateLimiting;
using Amazon.SimpleEmailV2;
using Amazon;
using Fitnutri.Application.Email;
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
        p.WithOrigins("https://fit-nutri.com") // ajuste se necessário
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// ===== DI =====
builder.Services.AddScoped<IAuthService, AuthService>();

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

app.UseHttpsRedirection();
app.UseCors("app");

app.UseRateLimiter();

// x-api-key para TODAS as rotas (inclui /auth/*). Se quiser liberar /auth/*, mova o middleware para um MapGroup.
app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

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


// ---------- HEALTH ----------

var commonGroup = app.MapGroup("/");

commonGroup.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
commonGroup.MapGet("/readyz", async (AppDbContext db, CancellationToken ct) =>
{
    var ok = await db.Database.CanConnectAsync(ct);
    return ok ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503);
});

app.Run();
