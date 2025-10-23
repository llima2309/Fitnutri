using SiteFitNutri.Services;
using SiteFitNutri.Services.Login;
using SiteFitNutri;
using SiteFitNutri.Components;

var builder = WebApplication.CreateBuilder(args);

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Necess�rio para acessar cookies da requisi��o atual
builder.Services.AddHttpContextAccessor();

// Handler que encaminha o cookie para a API
builder.Services.AddTransient<ForwardFitnutriCookieHandler>();

// HttpClient nomeado para a API
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https://api.fit-nutri.com");
    client.DefaultRequestHeaders.Add("x-api-key", "<STRONG_CLIENT_KEY>");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<ForwardFitnutriCookieHandler>(); // <-- importante

// ApiHttp usando o named client
builder.Services.AddScoped<IApiHttp>(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("api");
    return new ApiHttp(http);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.MapPost("/auth/cb", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var token = form["token"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    if (string.IsNullOrWhiteSpace(token))
        return Results.BadRequest("missing token");

    ctx.Response.Cookies.Append("fitnutri_auth", token, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,                 // prod = https
        SameSite = SameSiteMode.None,    // subdom�nios
        Domain = ".fit-nutri.com",     // ajuste ao seu dom�nio
        Path = "/",
        Expires = DateTimeOffset.UtcNow.AddHours(8)
    });

    return Results.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
});

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));


app.Run();
