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
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());
// EF Core
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var conn = builder.Configuration.GetConnectionString("Sql");
    opt.UseSqlServer(conn, sql => sql.EnableRetryOnFailure());
});

// Options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection("ApiKey"));
// Auth (JWT)
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetBytes(jwt.Key).Length < 32)
    throw new InvalidOperationException("Jwt:Key deve ter pelo menos 32 bytes (256 bits).");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;           // manter "sub" e "role"
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "role"                 // <--- importante
        };
    });
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});
// DI
builder.Services.AddScoped<IAuthService, AuthService>();

// Swagger + segurança (Bearer + x-api-key)
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


    // aplica o requisito em TODAS as operações
    c.OperationFilter<GlobalSecurityRequirementsOperationFilter>();
});

builder.Services.AddRateLimiter(options =>
{
    // política para registro por IP
    options.AddPolicy("register-ip", httpContext =>
    {
        var key = GetClientIp(httpContext) ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: key,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,                // até 10 registros/min por IP (ajuste conforme cenário)
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    // NOVA: login por IP (anti brute force)
    options.AddPolicy("login-ip", httpContext =>
    {
        var key = GetClientIp(httpContext) ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,            // 30 tentativas por minuto por IP (ajuste conforme seu cenário)
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    // você pode manter política global p/ outras rotas, se quiser
});

static string? GetClientIp(HttpContext ctx)
{
    // tenta X-Forwarded-For (se atrás de proxy/API Gateway), senão Connection.RemoteIp
    if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var xff) && !string.IsNullOrWhiteSpace(xff))
        return xff.ToString().Split(',')[0].Trim();

    return ctx.Connection.RemoteIpAddress?.ToString();
}
// AWS SES Client
builder.Services.AddSingleton<IAmazonSimpleEmailServiceV2>(_ =>
{
    var region = builder.Configuration["AWS:Region"] ?? "us-east-1";
    return new AmazonSimpleEmailServiceV2Client(RegionEndpoint.GetBySystemName(region));
});

// Email Sender
builder.Services.AddScoped<IEmailSender, AwsSesEmailSender>();
var app = builder.Build();

// Middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
// x-api-key para TODAS as rotas (inclui /auth/*)
app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Endpoints
// ====== GRUPO ADMIN ======
var adminGroup = app.MapGroup("/admin").WithTags("Admin");

// injeta o filtro com DI
adminGroup.RequireAuthorization("AdminOnly"); // <--- exige JWT com role=Admin
// Listar pendentes (pagina simples)
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

// Aprovar
adminGroup.MapPost("/users/{id:guid}/approve",
    async (Guid id, ApproveUserRequest req, AppDbContext db, IEmailSender emailSender, IConfiguration cfg, ILoggerFactory lf, CancellationToken ct) =>
    {
        var log = lf.CreateLogger("ApproveUser");
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (user is null) return Results.NotFound();

        if (user.Status == UserStatus.Approved)
            return Results.BadRequest(new { error = "Usuário já está aprovado." });

        user.Status = UserStatus.Approved;
        user.ApprovedAt = DateTime.UtcNow;
        user.ApprovedBy = string.IsNullOrWhiteSpace(req?.ApprovedBy) ? "admin" : req!.ApprovedBy;

        // 🔑 gera código int de 6 dígitos (0..999999)
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000);
        user.EmailVerificationCode = code; // armazenamos como int
                                           // EmailConfirmed permanece como estiver (provável false)


        var codeStr = code.ToString("D6"); // sempre 6 dígitos com zeros à esquerda
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
    });



// Rejeitar
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
// ====== FIM ADMIN ======

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
}).RequireRateLimiting("register-ip"); // <-- aplica rate limit



app.MapPost("/auth/login", async (IAuthService auth, LoginRequest req, CancellationToken ct) =>
{
    try
    {
        var (user, token, exp) = await auth.LoginAsync(req.UserNameOrEmail.Trim(), req.Password, ct);
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
}).RequireRateLimiting("login-ip");

app.MapGet("/users/me", async (AppDbContext db, HttpContext ctx, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();

    var id = Guid.Parse(sub);
    var user = await db.Users.FindAsync([id], ct);
    if (user is null) return Results.NotFound();

    return Results.Ok(new MeResponse(user.Id, user.UserName, user.Email, user.CreatedAt, user.EmailConfirmed, user.Status));

})
.RequireAuthorization();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapGet("/readyz", async (AppDbContext db, CancellationToken ct) =>
{
    var ok = await db.Database.CanConnectAsync(ct);
    return ok ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503);
});

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
})
.WithTags("Auth");



app.Run();

