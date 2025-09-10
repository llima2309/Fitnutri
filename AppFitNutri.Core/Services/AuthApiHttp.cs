using System.Net;
using System.Net.Http.Json;
using AppFitNutri.Core.Models;

namespace AppFitNutri.Core.Services;

public sealed class AuthApiHttp : IAuthApi
{
    private readonly HttpClient _http;

    public AuthApiHttp(HttpClient http) => _http = http;

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        using var response = await _http.PostAsJsonAsync("/auth/login", request, ct);

        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
            if (payload is null || string.IsNullOrWhiteSpace(payload.accessToken))
                return LoginResult.Fail("Resposta inválida do servidor.");
            return LoginResult.Success(payload.accessToken, payload.expiresAt);
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            try
            {
                var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: ct);
                if (problem is not null && problem.TryGetValue("error", out var message) && !string.IsNullOrWhiteSpace(message))
                    return LoginResult.Fail(message);
            }
            catch { /* Ignora parse e cai em erro genérico */ }
            return LoginResult.Fail("Credenciais inválidas.");
        }

        return LoginResult.Fail($"Erro {(int)response.StatusCode} ao autenticar.");
    }
}
