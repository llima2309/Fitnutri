using AppFitNutri.Models;
using AppFitNutri.Core.Services;
using System.Text.Json;

namespace AppFitNutri.Services;

public interface IProfissionaisService
{
    Task<List<Profissional>> GetProfissionaisByTipoAsync(int tipoProfissional);
}

public class ProfissionaisService : IProfissionaisService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStore _tokenStore;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProfissionaisService(HttpClient httpClient, ITokenStore tokenStore)
    {
        _httpClient = httpClient;
        _tokenStore = tokenStore;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<Profissional>> GetProfissionaisByTipoAsync(int tipoProfissional)
    {
        try
        {
            // Garantir que o usuário está autenticado antes de fazer a requisição
            await EnsureAuthenticatedAsync();
            
            var response = await _httpClient.GetAsync($"api/UserProfile/profissionais/{tipoProfissional}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var profissionais = JsonSerializer.Deserialize<List<Profissional>>(json, _jsonOptions);
                return profissionais ?? new List<Profissional>();
            }
            else
            {
                // Log do erro
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Erro na API: {response.StatusCode} - {errorContent}");
                return new List<Profissional>();
            }
        }
        catch (UnauthorizedAccessException)
        {
            System.Diagnostics.Debug.WriteLine("Usuário não autenticado ao buscar profissionais");
            return new List<Profissional>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao buscar profissionais: {ex.Message}");
            return new List<Profissional>();
        }
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
