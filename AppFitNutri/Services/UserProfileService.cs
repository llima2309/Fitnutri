using System.Text;
using System.Text.Json;
using AppFitNutri.Core.Models;

namespace AppFitNutri.Services;

// Modelos para deserializar o JSON da API
public class ApiGeneroOption
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class ApiEstadoOption
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty;
}

public interface IUserProfileService
{
    Task<UserProfileResponse?> GetProfileAsync();
    Task<UserProfileResponse?> CreateProfileAsync(CreateUserProfileRequest request);
    Task<AddressFromCepResponse?> GetAddressByCepAsync(string cep);
    Task<List<GeneroOption>> GetGeneroOptionsAsync();
    Task<List<EstadoOption>> GetEstadoOptionsAsync();
}

public class UserProfileService : IUserProfileService
{
    private readonly HttpClient _httpClient;
    private readonly SecureTokenStore _tokenStore;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserProfileService(HttpClient httpClient, SecureTokenStore tokenStore)
    {
        _httpClient = httpClient;
        _tokenStore = tokenStore;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<UserProfileResponse?> GetProfileAsync()
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.GetAsync("api/userprofile");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; // Usuário não tem perfil ainda
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserProfileResponse>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao buscar perfil: {ex.Message}");
            return null;
        }
    }

    public async Task<UserProfileResponse?> CreateProfileAsync(CreateUserProfileRequest request)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/userprofile", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserProfileResponse>(responseJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao criar perfil: {ex.Message}");
            throw;
        }
    }

    public async Task<AddressFromCepResponse?> GetAddressByCepAsync(string cep)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.GetAsync($"api/userprofile/cep/{cep}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AddressFromCepResponse>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao buscar CEP: {ex.Message}");
            return null;
        }
    }

    public async Task<List<GeneroOption>> GetGeneroOptionsAsync()
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            
            System.Diagnostics.Debug.WriteLine("==> Fazendo request para api/userprofile/options/genero");
            var response = await _httpClient.GetAsync("api/userprofile/options/genero");
            
            System.Diagnostics.Debug.WriteLine($"==> Status da resposta: {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"==> JSON recebido: {json}");
            
            var options = JsonSerializer.Deserialize<List<ApiGeneroOption>>(json, _jsonOptions);
            
            if (options == null || options.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("==> Nenhuma opção retornada da API, usando opções padrão");
                return GetDefaultGeneroOptions();
            }
            
            System.Diagnostics.Debug.WriteLine($"==> {options.Count} opções de gênero carregadas da API");
            return options.Select(o => new GeneroOption(o.Id, o.Nome)).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"==> Erro ao buscar opções de gênero da API: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"==> Stack trace: {ex.StackTrace}");
            return GetDefaultGeneroOptions();
        }
    }

    public async Task<List<EstadoOption>> GetEstadoOptionsAsync()
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.GetAsync("api/userprofile/options/estado");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = JsonSerializer.Deserialize<List<ApiEstadoOption>>(json, _jsonOptions);
            return options?.Select(o => new EstadoOption(o.Id, o.Nome, o.Sigla)).ToList() ?? new List<EstadoOption>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao buscar opções de estado: {ex.Message}");
            return GetDefaultEstadoOptions();
        }
    }

    private async Task SetAuthorizationHeaderAsync()
    {
        var token = await _tokenStore.GetAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static List<GeneroOption> GetDefaultGeneroOptions()
    {
        return new List<GeneroOption>
        {
            new(1, "Masculino"),
            new(2, "Feminino"),
            new(3, "Outro")
        };
    }

    private static List<EstadoOption> GetDefaultEstadoOptions()
    {
        return new List<EstadoOption>
        {
            new(1, "Acre", "AC"),
            new(2, "Alagoas", "AL"),
            new(3, "Amapá", "AP"),
            new(4, "Amazonas", "AM"),
            new(5, "Bahia", "BA"),
            new(6, "Ceará", "CE"),
            new(7, "Distrito Federal", "DF"),
            new(8, "Espírito Santo", "ES"),
            new(9, "Goiás", "GO"),
            new(10, "Maranhão", "MA"),
            new(11, "Mato Grosso", "MT"),
            new(12, "Mato Grosso do Sul", "MS"),
            new(13, "Minas Gerais", "MG"),
            new(14, "Pará", "PA"),
            new(15, "Paraíba", "PB"),
            new(16, "Paraná", "PR"),
            new(17, "Pernambuco", "PE"),
            new(18, "Piauí", "PI"),
            new(19, "Rio de Janeiro", "RJ"),
            new(20, "Rio Grande do Norte", "RN"),
            new(21, "Rio Grande do Sul", "RS"),
            new(22, "Rondônia", "RO"),
            new(23, "Roraima", "RR"),
            new(24, "Santa Catarina", "SC"),
            new(25, "São Paulo", "SP"),
            new(26, "Sergipe", "SE"),
            new(27, "Tocantins", "TO")
        };
    }
}
