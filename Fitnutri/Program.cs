using System.Text;
using Fitnutri.Auth;
using Fitnutri.Contracts;
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
        options.MapInboundClaims = false; // mantém "sub" como "sub"
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

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

var app = builder.Build();

// Middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// x-api-key para TODAS as rotas (inclui /auth/*)
app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Endpoints

app.MapPost("/auth/register", async (IAuthService auth, RegisterRequest req, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "Dados inválidos." });

    try
    {
        var user = await auth.RegisterAsync(req.UserName, req.Email, req.Password, ct);
        return Results.Created($"/users/{user.Id}", new { user.Id, user.UserName, user.Email, user.CreatedAt });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});



app.MapPost("/auth/login", async (IAuthService auth, LoginRequest req, CancellationToken ct) =>
{
    try
    {
        var (user, token, exp) = await auth.LoginAsync(req.UserNameOrEmail.Trim(), req.Password, ct);
        return Results.Ok(new AuthResponse(token, exp));
    }
    catch
    {
        return Results.Unauthorized();
    }
});

app.MapGet("/users/me", async (AppDbContext db, HttpContext ctx, CancellationToken ct) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value;
    if (sub is null) return Results.Unauthorized();

    var id = Guid.Parse(sub);
    var user = await db.Users.FindAsync([id], ct);
    if (user is null) return Results.NotFound();

    return Results.Ok(new MeResponse(user.Id, user.UserName, user.Email, user.CreatedAt));
})
.RequireAuthorization();

app.Run();
