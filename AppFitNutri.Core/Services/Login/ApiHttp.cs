using AppFitNutri.Core.Models;
using System.Net;
using System.Net.Http.Json;

namespace AppFitNutri.Core.Services.Login;
public interface IApiHttp
{
    Task<HttpResponseMessage> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<HttpResponseMessage> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<HttpResponseMessage> ValidaToken();
    Task<List<PendingUserDto>?> GetPendingUsersAsync(int skip = 0, int take = 20, CancellationToken ct = default);
    void SetBearerToken(string? token); // novo
}
// ApiHttp.cs (no Core)
public class ApiHttp : IApiHttp
{
    private readonly HttpClient _http;
    private readonly ITokenStore _tokens;

    public ApiHttp(HttpClient http)
    {
        _http = http;
    }
    public void SetBearerToken(string? token)
    {
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
    public Task<HttpResponseMessage> LoginAsync(LoginRequest req, CancellationToken ct = default)
        => _http.PostAsJsonAsync("/auth/login", req, ct);

    public Task<HttpResponseMessage> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    => _http.PostAsJsonAsync("/auth/register", req, ct);

    public Task<HttpResponseMessage> ValidaToken()
        => _http.GetAsync("/users/me");

    public Task<List<PendingUserDto>?> GetPendingUsersAsync(int skip = 0, int take = 20, CancellationToken ct = default) =>
    _http.GetFromJsonAsync<List<PendingUserDto>>($"/admin/users/pending?skip={skip}&take={take}", ct);
}


