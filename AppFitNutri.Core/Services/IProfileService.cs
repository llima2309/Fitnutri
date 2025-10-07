using System.Net.Http.Json;
using System.Text.Json;

namespace AppFitNutri.Core.Services;

public interface IProfileService
{
    Task<ProfileResponse> AssociarPerfilAsync(int tipoPerfil, CancellationToken cancellationToken = default);
    Task<List<ProfileResponse>> ObterMeusPerfisAsync(CancellationToken cancellationToken = default);
    Task<List<TipoPerfilDisponivel>> ObterTiposDisponiveisAsync(CancellationToken cancellationToken = default);
    Task<bool> RemoverPerfilAsync(Guid perfilId, CancellationToken cancellationToken = default);
}

public class ProfileService : IProfileService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStore _tokenStore;

    public ProfileService(HttpClient httpClient, ITokenStore tokenStore)
    {
        _httpClient = httpClient;
        _tokenStore = tokenStore;
    }

    public async Task<ProfileResponse> AssociarPerfilAsync(int tipoPerfil, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var request = new { TipoPerfil = tipoPerfil };
        var response = await _httpClient.PostAsJsonAsync("/user/perfil/associar", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AssociarPerfilResponse>(cancellationToken);
            return result?.Perfil ?? throw new InvalidOperationException("Resposta inválida da API");
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var errorMessage = "Erro ao associar perfil";

        try
        {
            var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
            if (errorObj?.TryGetValue("error", out var message) == true)
            {
                errorMessage = message;
            }
        }
        catch
        {
            // Usar mensagem padrão se não conseguir deserializar
        }

        throw new InvalidOperationException(errorMessage);
    }

    public async Task<List<ProfileResponse>> ObterMeusPerfisAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.GetAsync("/user/perfil/meus-perfis", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var perfis = await response.Content.ReadFromJsonAsync<List<ProfileResponse>>(cancellationToken);
            return perfis ?? new List<ProfileResponse>();
        }

        throw new InvalidOperationException("Erro ao obter perfis do usuário");
    }

    public async Task<List<TipoPerfilDisponivel>> ObterTiposDisponiveisAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/user/perfil/tipos-disponiveis", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var tipos = await response.Content.ReadFromJsonAsync<List<TipoPerfilDisponivel>>(cancellationToken);
            return tipos ?? new List<TipoPerfilDisponivel>();
        }

        throw new InvalidOperationException("Erro ao obter tipos de perfil disponíveis");
    }

    public async Task<bool> RemoverPerfilAsync(Guid perfilId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.DeleteAsync($"/user/perfil/remover/{perfilId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private async Task EnsureAuthenticatedAsync()
    {
        var token = await _tokenStore.GetAsync();
        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("Usuário não autenticado");
        }

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}

// DTOs para comunicação com a API
public class ProfileResponse
{
    public Guid Id { get; set; }
    public int Tipo { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class AssociarPerfilResponse
{
    public string Message { get; set; } = string.Empty;
    public ProfileResponse Perfil { get; set; } = new();
}

public class TipoPerfilDisponivel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}
