using AppFitNutri.Core.Models;
using System.Net;
using System.Net.Http.Json;

namespace AppFitNutri.Core.Services.Login;
public interface IApiHttp
{
    Task<HttpResponseMessage> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<HttpResponseMessage> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<HttpResponseMessage> ValidaToken();
    Task<HttpResponseMessage> ApproveUserAsync(Guid userId, CancellationToken ct = default);
    Task<HttpResponseMessage> RejectUserAsync(Guid userId);
    Task<HttpResponseMessage> DeleteUserAsync(Guid userId);
    Task<List<PendingUserDto>?> GetPendingUsersAsync(int skip = 0, int take = 20, CancellationToken ct = default);
    Task<List<PendingUserDto>?> GetApprovedUsersAsync(int skip = 0, int take = 20, CancellationToken ct = default);
    Task<List<PendingUserDto>?> GetRejectedUsersAsync(int skip = 0, int take = 20, CancellationToken ct = default);

    
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
   
    public Task<HttpResponseMessage> LoginAsync(LoginRequest req, CancellationToken ct = default)
        => _http.PostAsJsonAsync("/auth/login", req, ct);

    public Task<HttpResponseMessage> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    => _http.PostAsJsonAsync("/auth/register", req, ct);

    public Task<HttpResponseMessage> ValidaToken()
        => _http.GetAsync("/users/me");

    public Task<List<PendingUserDto>?> GetPendingUsersAsync(int skip = 0, int take = 20, CancellationToken ct = default) =>
    _http.GetFromJsonAsync<List<PendingUserDto>>($"/admin/users/pending?skip={skip}&take={take}", ct);

    public Task<List<PendingUserDto>?> GetApprovedUsersAsync(int skip = 0, int take = 20, CancellationToken ct = default)
    => _http.GetFromJsonAsync<List<PendingUserDto>>($"/admin/users/approved?skip={skip}&take={take}", ct);
    public Task<List<PendingUserDto>?> GetRejectedUsersAsync(int skip = 0, int take = 20, CancellationToken ct = default)
    => _http.GetFromJsonAsync<List<PendingUserDto>>($"/admin/users/rejects?skip={skip}&take={take}", ct);
    public Task<HttpResponseMessage> ApproveUserAsync(Guid userId, CancellationToken ct = default)
        => _http.PostAsJsonAsync(
            $"/admin/users/{userId}/approve",
            new { ApproveUser = "admin" }, // body correto
            ct);

    public Task<HttpResponseMessage> RejectUserAsync(Guid userId)
  => _http.PostAsJsonAsync<object>($"/admin/users/{userId}/reject", new { ApproveUser = "admin" },CancellationToken.None);

    public Task<HttpResponseMessage> DeleteUserAsync(Guid userId)
  => _http.DeleteAsync($"/admin/users/{userId}");
}


