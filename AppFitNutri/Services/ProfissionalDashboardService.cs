using System.Text.Json;
using AppFitNutri.Core.Services;
using AppFitNutri.Models;

namespace AppFitNutri.Services;

public interface IProfissionalDashboardService
{
    Task<DashboardNutricionistaDto?> GetDashboardAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AgendamentoDto>> GetAgendamentosProfissionalAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PacienteDto>> GetPacientesAsync(CancellationToken ct = default);
}

public class ProfissionalDashboardService : IProfissionalDashboardService
{
    private readonly HttpClient _http;
    private readonly ITokenStore _tokenStore;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProfissionalDashboardService(HttpClient http, ITokenStore tokenStore)
    {
        _http = http;
        _tokenStore = tokenStore;
    }

    public async Task<DashboardNutricionistaDto?> GetDashboardAsync(CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        
        try
        {
            // Por enquanto, vamos simular dados - depois implementar endpoint na API
            var agendamentos = await GetAgendamentosProfissionalAsync(ct);
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var inicioSemana = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek));
            var fimSemana = inicioSemana.AddDays(6);

            var agendamentosHoje = agendamentos.Where(a => a.Data == hoje).Count();
            var agendamentosSemana = agendamentos.Where(a => a.Data >= inicioSemana && a.Data <= fimSemana).Count();
            var proximosAgendamentos = agendamentos
                .Where(a => a.Data >= hoje)
                .OrderBy(a => a.Data).ThenBy(a => a.Hora)
                .Take(5)
                .ToList();

            var pacientes = await GetPacientesAsync(ct);

            return new DashboardNutricionistaDto(
                agendamentosHoje,
                agendamentosSemana,
                pacientes.Count,
                0, // TODO: implementar contagem de dietas
                proximosAgendamentos
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao buscar dashboard: {ex.Message}");
            return null;
        }
    }

    public async Task<IReadOnlyList<AgendamentoDto>> GetAgendamentosProfissionalAsync(CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        
        try
        {
            var resp = await _http.GetAsync("/agendamentos/profissional/me", ct);
            if (!resp.IsSuccessStatusCode) 
            {
                // Se o endpoint não existir ainda, retorna lista vazia
                return Array.Empty<AgendamentoDto>();
            }
            
            var json = await resp.Content.ReadAsStringAsync(ct);
            var items = JsonSerializer.Deserialize<List<AgendamentoDto>>(json, _jsonOptions) ?? new List<AgendamentoDto>();
            return items;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao buscar agendamentos do profissional: {ex.Message}");
            return Array.Empty<AgendamentoDto>();
        }
    }

    public async Task<IReadOnlyList<PacienteDto>> GetPacientesAsync(CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        
        try
        {
            var resp = await _http.GetAsync("/profissional/pacientes", ct);
            if (!resp.IsSuccessStatusCode)
            {
                // Se o endpoint não existir ainda, retorna lista vazia
                return Array.Empty<PacienteDto>();
            }
            
            var json = await resp.Content.ReadAsStringAsync(ct);
            var items = JsonSerializer.Deserialize<List<PacienteDto>>(json, _jsonOptions) ?? new List<PacienteDto>();
            return items;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao buscar pacientes: {ex.Message}");
            return Array.Empty<PacienteDto>();
        }
    }

    private async Task EnsureAuthAsync()
    {
        var token = await _tokenStore.GetAsync();
        if (string.IsNullOrWhiteSpace(token)) throw new UnauthorizedAccessException();
        _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}

