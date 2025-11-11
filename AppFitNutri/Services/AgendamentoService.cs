using System.Text.Json;
using AppFitNutri.Core.Services;
using System.Net.Http.Json;
using AppFitNutri.Models;

namespace AppFitNutri.Services;

public interface IAgendamentoService
{
    Task<IReadOnlyList<string>> GetDisponibilidadeAsync(Guid profissionalId, DateTime data, CancellationToken ct = default);
    Task<(bool ok, string? error)> CriarAgendamentoAsync(Guid profissionalId, DateTime data, string hora, CancellationToken ct = default);
    Task<(bool ok, string? error)> ConfirmarAgendamentoAsync(Guid agendamentoId, CancellationToken ct = default);
    Task<(bool ok, string? error)> CancelarAgendamentoAsync(Guid agendamentoId, CancellationToken ct = default);
    Task<(bool ok, string? error)> DeletarAgendamentoAsync(Guid agendamentoId, CancellationToken ct = default);
    Task<IReadOnlyList<AppFitNutri.Models.AgendamentoDto>> GetMeusAgendamentosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AppFitNutri.Models.AgendamentoDto>> GetAgendamentosProfissionalAsync(CancellationToken ct = default);
}

public class AgendamentoService : IAgendamentoService
{
    private readonly HttpClient _http;
    private readonly ITokenStore _tokenStore;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AgendamentoService(HttpClient http, ITokenStore tokenStore)
    {
        _http = http;
        _tokenStore = tokenStore;
    }

    public async Task<IReadOnlyList<string>> GetDisponibilidadeAsync(Guid profissionalId, DateTime data, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var dateStr = data.ToString("yyyy-MM-dd");
        var resp = await _http.GetAsync($"/agendamentos/disponibilidade?profissionalId={profissionalId}&data={dateStr}", ct);
        if (!resp.IsSuccessStatusCode) return Array.Empty<string>();
        var json = await resp.Content.ReadAsStringAsync(ct);
        var dto = JsonSerializer.Deserialize<DisponibilidadeDto>(json, _jsonOptions);
        return dto?.Horarios ?? Array.Empty<string>();
    }

    public async Task<(bool ok, string? error)> CriarAgendamentoAsync(Guid profissionalId, DateTime data, string hora, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var body = new { ProfissionalId = profissionalId, Data = DateOnly.FromDateTime(data), Hora = TimeOnly.Parse(hora) };
        var resp = await _http.PostAsJsonAsync("/agendamentos", body, _jsonOptions, ct);
        if (resp.IsSuccessStatusCode) return (true, null);
        var err = await resp.Content.ReadAsStringAsync(ct);
        return (false, string.IsNullOrWhiteSpace(err) ? resp.StatusCode.ToString() : err);
    }

    public async Task<(bool ok, string? error)> ConfirmarAgendamentoAsync(Guid agendamentoId, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var resp = await _http.PutAsync($"/agendamentos/{agendamentoId}/confirmar", null, ct);
        if (resp.IsSuccessStatusCode) return (true, null);
        var err = await resp.Content.ReadAsStringAsync(ct);
        return (false, string.IsNullOrWhiteSpace(err) ? resp.StatusCode.ToString() : err);
    }

    public async Task<(bool ok, string? error)> CancelarAgendamentoAsync(Guid agendamentoId, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var resp = await _http.PutAsync($"/agendamentos/{agendamentoId}/cancelar", null, ct);
        if (resp.IsSuccessStatusCode) return (true, null);
        var err = await resp.Content.ReadAsStringAsync(ct);
        return (false, string.IsNullOrWhiteSpace(err) ? resp.StatusCode.ToString() : err);
    }

    public async Task<(bool ok, string? error)> DeletarAgendamentoAsync(Guid agendamentoId, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var resp = await _http.DeleteAsync($"/agendamentos/{agendamentoId}", ct);
        if (resp.IsSuccessStatusCode || resp.StatusCode == System.Net.HttpStatusCode.NoContent) return (true, null);
        var err = await resp.Content.ReadAsStringAsync(ct);
        return (false, string.IsNullOrWhiteSpace(err) ? resp.StatusCode.ToString() : err);
    }

    public async Task<IReadOnlyList<AppFitNutri.Models.AgendamentoDto>> GetMeusAgendamentosAsync(CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var resp = await _http.GetAsync("/agendamentos/me", ct);
        if (!resp.IsSuccessStatusCode) return Array.Empty<AppFitNutri.Models.AgendamentoDto>();
        var json = await resp.Content.ReadAsStringAsync(ct);
        var items = JsonSerializer.Deserialize<List<AppFitNutri.Models.AgendamentoDto>>(json, _jsonOptions) ?? new List<AppFitNutri.Models.AgendamentoDto>();
        return items;
    }

    public async Task<IReadOnlyList<AppFitNutri.Models.AgendamentoDto>> GetAgendamentosProfissionalAsync(CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var resp = await _http.GetAsync("/agendamentos/profissional/me", ct);
        if (!resp.IsSuccessStatusCode) return Array.Empty<AppFitNutri.Models.AgendamentoDto>();
        var json = await resp.Content.ReadAsStringAsync(ct);
        var items = JsonSerializer.Deserialize<List<AppFitNutri.Models.AgendamentoDto>>(json, _jsonOptions) ?? new List<AppFitNutri.Models.AgendamentoDto>();
        return items;
    }

    private async Task EnsureAuthAsync()
    {
        var token = await _tokenStore.GetAsync();
        if (string.IsNullOrWhiteSpace(token)) throw new UnauthorizedAccessException();
        _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private record DisponibilidadeDto(IReadOnlyList<string> Horarios);
}
