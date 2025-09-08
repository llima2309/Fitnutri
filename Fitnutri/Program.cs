using System.Text;
using System.Threading.RateLimiting;
using Fitnutri.Auth;
using Fitnutri.Contracts;
using Fitnutri.Domain;
using Fitnutri.Infrastructure;
using Fitnutri.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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

// Swagger + seguran�a (Bearer + x-api-key)
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


    // aplica o requisito em TODAS as opera��es
    c.OperationFilter<GlobalSecurityRequirementsOperationFilter>();
});

builder.Services.AddRateLimiter(options =>
{
    // pol�tica para registro por IP
    options.AddPolicy("register-ip", httpContext =>
    {
        var key = GetClientIp(httpContext) ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: key,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,                // at� 10 registros/min por IP (ajuste conforme cen�rio)
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
                PermitLimit = 30,            // 30 tentativas por minuto por IP (ajuste conforme seu cen�rio)
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    // voc� pode manter pol�tica global p/ outras rotas, se quiser
});

static string? GetClientIp(HttpContext ctx)
{
    // tenta X-Forwarded-For (se atr�s de proxy/API Gateway), sen�o Connection.RemoteIp
    if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var xff) && !string.IsNullOrWhiteSpace(xff))
        return xff.ToString().Split(',')[0].Trim();

    return ctx.Connection.RemoteIpAddress?.ToString();
}

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
adminGroup.MapPost("/users/{id:guid}/approve", async (Guid id, ApproveUserRequest req, AppDbContext db, CancellationToken ct) =>
{
    var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
    if (user is null) return Results.NotFound();

    if (user.Status == UserStatus.Approved)
        return Results.BadRequest(new { error = "Usu�rio j� est� aprovado." });

    user.Status = UserStatus.Approved;
    user.ApprovedAt = DateTime.UtcNow;
    user.ApprovedBy = string.IsNullOrWhiteSpace(req?.ApprovedBy) ? "admin" : req!.ApprovedBy;

    await db.SaveChangesAsync(ct);
    return Results.Ok(new { message = "Usu�rio aprovado.", user.Id, user.Status, user.ApprovedAt, user.ApprovedBy });
});

// Rejeitar
adminGroup.MapPost("/users/{id:guid}/reject", async (Guid id, RejectUserRequest req, AppDbContext db, CancellationToken ct) =>
{
    var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
    if (user is null) return Results.NotFound();

    if (user.Status == UserStatus.Rejected)
        return Results.BadRequest(new { error = "Usu�rio j� est� rejeitado." });

    user.Status = UserStatus.Rejected;
    user.ApprovedAt = DateTime.UtcNow;
    user.ApprovedBy = string.IsNullOrWhiteSpace(req?.ApprovedBy) ? "admin" : req!.ApprovedBy;

    await db.SaveChangesAsync(ct);
    return Results.Ok(new { message = "Usu�rio rejeitado.", user.Id, user.Status, user.ApprovedAt, user.ApprovedBy, req?.Reason });
});
// ====== FIM ADMIN ======

app.MapPost("/auth/register", async (IAuthService auth, RegisterRequest req, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "Dados inv�lidos." });

    try
    {
        var user = await auth.RegisterAsync(req.UserName, req.Email, req.Password, ct);
        return Results.Created($"/users/{user.Id}", new { message = "Usu�rio criado. Aguarde aprova��o.", userId = user.Id });
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
    catch (InvalidOperationException ex) when (ex.Message.Contains("Usu�rio n�o aprovado"))
    {
        return Results.BadRequest(new { error = ex.Message });

    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("E-mail n�o verificado"))
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


app.Run();
