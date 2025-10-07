using Fitnutri.Contracts;
using System.Text.Json;

namespace Fitnutri.Application.Services;

public interface IViaCepService
{
    Task<AddressFromCepResponse?> GetAddressByCepAsync(string cep);
}

public class ViaCepService : IViaCepService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<AddressFromCepResponse?> GetAddressByCepAsync(string cep)
    {
        try
        {
            // Remove caracteres não numéricos do CEP
            var cleanCep = new string(cep.Where(char.IsDigit).ToArray());
            
            if (cleanCep.Length != 8)
            {
                _logger.LogWarning("CEP inválido: {Cep}", cep);
                return null;
            }

            var url = $"https://viacep.com.br/ws/{cleanCep}/json/";
            
            _logger.LogInformation("Consultando ViaCEP para CEP: {Cep}", cleanCep);
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao consultar ViaCEP. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            
            // Verifica se a resposta contém erro (quando o CEP não é encontrado, a API retorna {"erro": true})
            if (content.Contains("\"erro\""))
            {
                _logger.LogWarning("CEP não encontrado: {Cep}", cleanCep);
                return null;
            }
            
            var viaCepResponse = JsonSerializer.Deserialize<ViaCepResponse>(content, _jsonOptions);

            if (viaCepResponse == null || string.IsNullOrEmpty(viaCepResponse.Cep))
            {
                _logger.LogWarning("CEP não encontrado: {Cep}", cleanCep);
                return null;
            }

            return new AddressFromCepResponse(
                CEP: FormatCep(cleanCep),
                Logradouro: viaCepResponse.Logradouro ?? string.Empty,
                Complemento: string.IsNullOrWhiteSpace(viaCepResponse.Complemento) ? null : viaCepResponse.Complemento,
                Bairro: viaCepResponse.Bairro ?? string.Empty,
                Cidade: viaCepResponse.Localidade ?? string.Empty,
                UF: viaCepResponse.Uf ?? string.Empty,
                Estado: viaCepResponse.Estado ?? string.Empty,
                DDD: viaCepResponse.Ddd ?? string.Empty
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar CEP: {Cep}", cep);
            return null;
        }
    }

    private static string FormatCep(string cep)
    {
        if (cep.Length == 8)
        {
            return $"{cep.Substring(0, 5)}-{cep.Substring(5)}";
        }
        return cep;
    }
}
