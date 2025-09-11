using AppFitNutri.Core.Services;
using AppFitNutri.Core.Services.Login;
using SiteFitNutri;
using SiteFitNutri.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
// 1) Named HttpClient “api” com base address/headers
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https://api.fit-nutri.com");
    client.DefaultRequestHeaders.Add("x-api-key", "<STRONG_CLIENT_KEY>");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// 2) ApiHttp Scoped, construído com o named client
builder.Services.AddScoped<IApiHttp>(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("api");
    return new ApiHttp(http); // ApiHttp guarda o DefaultRequestHeaders
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();



app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
