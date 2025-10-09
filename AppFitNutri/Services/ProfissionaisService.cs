using AppFitNutri.Models;
using System.Text.Json;

namespace AppFitNutri.Services;

public interface IProfissionaisService
{
    Task<List<Profissional>> GetProfissionaisByTipoAsync(int tipoProfissional);
}

public class ProfissionaisService : IProfissionaisService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProfissionaisService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<Profissional>> GetProfissionaisByTipoAsync(int tipoProfissional)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/userprofile/profissionais/{tipoProfissional}");
            
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao buscar profissionais: {ex.Message}");
            return new List<Profissional>();
        }
    }
}
