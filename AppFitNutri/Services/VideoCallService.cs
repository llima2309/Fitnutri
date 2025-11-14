using System.Net.Http.Json;
using System.Text.Json;
using AppFitNutri.Core.Services;

namespace AppFitNutri.Services;

public interface IVideoCallService
{
    Task<VideoCallResponse?> IniciarChamadaAsync(Guid agendamentoId, CancellationToken ct = default);
    Task<(bool ok, string? error)> EncerrarChamadaAsync(Guid agendamentoId, CancellationToken ct = default);
    Task<VideoCallStatusResponse?> GetStatusChamadaAsync(Guid agendamentoId, CancellationToken ct = default);
}

public class VideoCallService : IVideoCallService
{
    private readonly HttpClient _http;
    private readonly ITokenStore _tokenStore;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public VideoCallService(HttpClient http, ITokenStore tokenStore)
    {
        _http = http;
        _tokenStore = tokenStore;
    }

    public async Task<VideoCallResponse?> IniciarChamadaAsync(Guid agendamentoId, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        
        var body = new { AgendamentoId = agendamentoId };
        var resp = await _http.PostAsJsonAsync("/api/videocall/initiate", body, _jsonOptions, ct);
        
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync(ct);
            System.Diagnostics.Debug.WriteLine($"Erro ao iniciar chamada: {resp.StatusCode} - {error}");
            return null;
        }

        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<VideoCallResponse>(json, _jsonOptions);
    }

    public async Task<(bool ok, string? error)> EncerrarChamadaAsync(Guid agendamentoId, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        
        var body = new { AgendamentoId = agendamentoId };
        var resp = await _http.PostAsJsonAsync("/api/videocall/end", body, _jsonOptions, ct);
        
        if (resp.IsSuccessStatusCode) 
            return (true, null);
        
        var err = await resp.Content.ReadAsStringAsync(ct);
        return (false, string.IsNullOrWhiteSpace(err) ? resp.StatusCode.ToString() : err);
    }

    public async Task<VideoCallStatusResponse?> GetStatusChamadaAsync(Guid agendamentoId, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        
        var resp = await _http.GetAsync($"/api/videocall/status/{agendamentoId}", ct);
        
        if (!resp.IsSuccessStatusCode)
            return null;

        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<VideoCallStatusResponse>(json, _jsonOptions);
    }

    private async Task EnsureAuthAsync()
    {
        var token = await _tokenStore.GetAsync();
        if (string.IsNullOrWhiteSpace(token)) 
            throw new UnauthorizedAccessException("Token não encontrado");
        
        _http.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}

// DTOs
public record VideoCallResponse(
    Guid AgendamentoId,
    string CallToken,
    DateTime CallStartedAt,
    string HubUrl
);

public record VideoCallStatusResponse(
    Guid AgendamentoId,
    bool IsActive,
    DateTime? CallStartedAt,
    DateTime? CallEndedAt,
    int? DurationMinutes
);

