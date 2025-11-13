using System.Net.Http.Json;
using System.Text.Json;
using AppFitNutri.Core.Services;
using AppFitNutri.Models;

namespace AppFitNutri.Services;

public interface IDietService
{
    Task<(bool ok, string? error, Guid? dietId)> CreateDietAsync(CreateDietDto dto, CancellationToken ct = default);
    Task<List<DietSummaryDto>> GetMyDietsAsync(CancellationToken ct = default);
    Task<DietDetailDto?> GetDietByIdAsync(Guid dietId, CancellationToken ct = default);
    Task<(bool ok, string? error)> UpdateDietAsync(Guid dietId, UpdateDietDto dto, CancellationToken ct = default);
    Task<(bool ok, string? error)> DeleteDietAsync(Guid dietId, CancellationToken ct = default);
    Task<(bool ok, string? error)> AssignDietToPatientAsync(AssignDietDto dto, CancellationToken ct = default);
    Task<List<PatientDietDto>> GetDietPatientsAsync(Guid dietId, CancellationToken ct = default);
    Task<(bool ok, string? error)> DeactivateDietForPatientAsync(Guid dietId, Guid patientUserId, CancellationToken ct = default);
}

public class DietService : IDietService
{
    private readonly HttpClient _http;
    private readonly ITokenStore _tokenStore;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DietService(HttpClient http, ITokenStore tokenStore)
    {
        _http = http;
        _tokenStore = tokenStore;
    }

    public async Task<(bool ok, string? error, Guid? dietId)> CreateDietAsync(CreateDietDto dto, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var resp = await _http.PostAsJsonAsync("/dietas", dto, _jsonOptions, ct);
        
        if (resp.IsSuccessStatusCode)
        {
            var json = await resp.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<DietDetailDto>(json, _jsonOptions);
            return (true, null, result?.Id);
        }
        
        var err = await resp.Content.ReadAsStringAsync(ct);
        return (false, string.IsNullOrWhiteSpace(err) ? resp.StatusCode.ToString() : err, null);
    }

    public async Task<List<DietSummaryDto>> GetMyDietsAsync(CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var resp = await _http.GetAsync("/dietas", ct);
        
        if (!resp.IsSuccessStatusCode) 
            return new List<DietSummaryDto>();
        
        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<List<DietSummaryDto>>(json, _jsonOptions) ?? new List<DietSummaryDto>();
    }

    public async Task<DietDetailDto?> GetDietByIdAsync(Guid dietId, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var resp = await _http.GetAsync($"/dietas/{dietId}", ct);
        
        if (!resp.IsSuccessStatusCode) 
            return null;
        
        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<DietDetailDto>(json, _jsonOptions);
    }

    public async Task<(bool ok, string? error)> UpdateDietAsync(Guid dietId, UpdateDietDto dto, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        string teste = JsonSerializer.Serialize(dto, _jsonOptions);
        var resp = await _http.PutAsJsonAsync($"/dietas/{dietId}", dto, _jsonOptions, ct);
        
        if (resp.IsSuccessStatusCode) 
            return (true, null);
        
        var err = await resp.Content.ReadAsStringAsync(ct);
        return (false, string.IsNullOrWhiteSpace(err) ? resp.StatusCode.ToString() : err);
    }

    public async Task<(bool ok, string? error)> DeleteDietAsync(Guid dietId, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var resp = await _http.DeleteAsync($"/dietas/{dietId}", ct);
        
        if (resp.IsSuccessStatusCode || resp.StatusCode == System.Net.HttpStatusCode.NoContent) 
            return (true, null);
        
        var err = await resp.Content.ReadAsStringAsync(ct);
        return (false, string.IsNullOrWhiteSpace(err) ? resp.StatusCode.ToString() : err);
    }

    public async Task<(bool ok, string? error)> AssignDietToPatientAsync(AssignDietDto dto, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var resp = await _http.PostAsJsonAsync("/dietas/assign", dto, _jsonOptions, ct);
        
        if (resp.IsSuccessStatusCode) 
            return (true, null);
        
        var err = await resp.Content.ReadAsStringAsync(ct);
        return (false, string.IsNullOrWhiteSpace(err) ? resp.StatusCode.ToString() : err);
    }

    public async Task<List<PatientDietDto>> GetDietPatientsAsync(Guid dietId, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var resp = await _http.GetAsync($"/dietas/{dietId}/patients", ct);
        
        if (!resp.IsSuccessStatusCode) 
            return new List<PatientDietDto>();
        
        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<List<PatientDietDto>>(json, _jsonOptions) ?? new List<PatientDietDto>();
    }

    public async Task<(bool ok, string? error)> DeactivateDietForPatientAsync(Guid dietId, Guid patientUserId, CancellationToken ct = default)
    {
        await EnsureAuthAsync();
        var resp = await _http.PostAsync($"/dietas/{dietId}/deactivate/{patientUserId}", null, ct);
        
        if (resp.IsSuccessStatusCode || resp.StatusCode == System.Net.HttpStatusCode.NoContent) 
            return (true, null);
        
        var err = await resp.Content.ReadAsStringAsync(ct);
        return (false, string.IsNullOrWhiteSpace(err) ? resp.StatusCode.ToString() : err);
    }

    private async Task EnsureAuthAsync()
    {
        var token = await _tokenStore.GetAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            _http.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}

// ===== DTOs =====

public class CreateDietDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DietType Type { get; set; }
    public List<DayMealDto> DayMeals { get; set; } = new();
}

public class UpdateDietDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DietType? Type { get; set; }
    public List<DayMealDto>? DayMeals { get; set; }
}

public class DayMealDto
{
    public string Day { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Breakfast { get; set; } = string.Empty;
    public string MorningSnack { get; set; } = string.Empty;
    public string Lunch { get; set; } = string.Empty;
    public string AfternoonSnack { get; set; } = string.Empty;
    public string Dinner { get; set; } = string.Empty;
}

public class AssignDietDto
{
    public Guid DietId { get; set; }
    public Guid PatientUserId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class DietSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DietType Type { get; set; }
    public int PatientsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DietDetailDto
{
    public Guid Id { get; set; }
    public Guid ProfissionalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DietType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<DayMealDetailDto> DayMeals { get; set; } = new();
    public int PatientsCount { get; set; }
}

public class DayMealDetailDto
{
    public Guid Id { get; set; }
    public string Day { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public MealDetailDto Meals { get; set; } = new();
}

public class MealDetailDto
{
    public string Breakfast { get; set; } = string.Empty;
    public string MorningSnack { get; set; } = string.Empty;
    public string Lunch { get; set; } = string.Empty;
    public string AfternoonSnack { get; set; } = string.Empty;
    public string Dinner { get; set; } = string.Empty;
}

public class PatientDietDto
{
    public Guid Id { get; set; }
    public Guid PatientUserId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DietId { get; set; }
    public string DietTitle { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
}

