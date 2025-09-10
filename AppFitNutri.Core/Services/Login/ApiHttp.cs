using AppFitNutri.Core.Models;
using System.Net;
using System.Net.Http.Json;

namespace AppFitNutri.Core.Services.Login;
public interface IApiHttp
{
    Task<HttpResponseMessage> PostAsyncLogin(LoginRequest request, CancellationToken ct);
}
public sealed class ApiHttp : IApiHttp
{
    private readonly HttpClient _http;
    public ApiHttp(HttpClient http) => _http = http;

    public async Task<HttpResponseMessage> PostAsyncLogin(LoginRequest request, CancellationToken ct)
    {
        return await _http.PostAsJsonAsync("/auth/login", request, ct);
    }
}

