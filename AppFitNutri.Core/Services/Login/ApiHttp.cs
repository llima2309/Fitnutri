using AppFitNutri.Core.Models;
using System.Net;
using System.Net.Http.Json;

namespace AppFitNutri.Core.Services.Login;
public interface IApiHttp
{
    Task<HttpResponseMessage> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<HttpResponseMessage> RegisterAsync(RegisterRequest request, CancellationToken ct);

    Task<HttpResponseMessage> ValidaToken();


}
public sealed class ApiHttp : IApiHttp
{
    private readonly HttpClient _http;
    public ApiHttp(HttpClient http) => _http = http;

    public async Task<HttpResponseMessage> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        return await _http.PostAsJsonAsync("/auth/login", request, ct);
    }

    public async Task<HttpResponseMessage> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        return await _http.PostAsJsonAsync("/auth/register", request, ct);
    }

    public async Task<HttpResponseMessage> ValidaToken()
    {
        return await _http.GetAsync("/users/me");
    }
}

