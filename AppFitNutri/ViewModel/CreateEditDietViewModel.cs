using System.Collections.ObjectModel;
using AppFitNutri.Models;
using AppFitNutri.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppFitNutri.ViewModel;

[QueryProperty(nameof(DietId), "dietId")]
public partial class CreateEditDietViewModel : ObservableObject
{
    private readonly IDietService _dietService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private Guid? dietId;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private DietType selectedType = DietType.Keto;

    [ObservableProperty]
    private DietTypeOption? selectedDietType;

    public ObservableCollection<DayMeal> DayMeals { get; } = new();

    public bool IsEditMode => DietId.HasValue;
    public string PageTitle => IsEditMode ? "Editar Dieta" : "Nova Dieta";
    public string SaveButtonText => IsEditMode ? "Atualizar Dieta" : "Criar Dieta";

    public List<DietTypeOption> DietTypes { get; } = new()
    {
        new() { Type = DietType.Keto, Label = "Keto", Icon = "🔥" },
        new() { Type = DietType.LowCarb, Label = "Low Carb", Icon = "🥗" },
        new() { Type = DietType.Vegan, Label = "Vegana", Icon = "🌱" },
        new() { Type = DietType.Celiac, Label = "Celíaca", Icon = "🌾" },
        new() { Type = DietType.Vegetarian, Label = "Vegetariana", Icon = "🥕" }
    };

    public CreateEditDietViewModel(IDietService dietService)
    {
        _dietService = dietService;
        InitializeDayMeals();
        SelectedDietType = DietTypes[0]; // Default to Keto
    }

    partial void OnDietIdChanged(Guid? value)
    {
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(SaveButtonText));

        if (value.HasValue)
        {
            _ = LoadDietCommand.ExecuteAsync(null);
        }
    }

    private void InitializeDayMeals()
    {
        var days = new[] { "SEG", "TER", "QUA", "QUI", "SEX", "SAB", "DOM" };
        var dayLabels = new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira", "Sábado", "Domingo" };
        var colors = new[] { "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8", "#F7DC6F", "#BB8FCE" };

        DayMeals.Clear();
        for (int i = 0; i < days.Length; i++)
        {
            DayMeals.Add(new DayMeal
            {
                Day = days[i],
                DayLabel = dayLabels[i],
                Color = colors[i],
                Breakfast = string.Empty,
                MorningSnack = string.Empty,
                Lunch = string.Empty,
                AfternoonSnack = string.Empty,
                Dinner = string.Empty,
                IsExpanded = false
            });
        }
    }

    [RelayCommand]
    private async Task LoadDiet()
    {
        if (!DietId.HasValue) return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var diet = await _dietService.GetDietByIdAsync(DietId.Value);
            if (diet == null)
            {
                ErrorMessage = "Dieta não encontrada";
                return;
            }

            Title = diet.Title;
            Description = diet.Description;
            SelectedType = diet.Type;
            SelectedDietType = DietTypes.FirstOrDefault(dt => dt.Type == diet.Type);

            // Carregar refeições
            foreach (var dayMeal in diet.DayMeals)
            {
                var model = DayMeals.FirstOrDefault(d => d.Day == dayMeal.Day);
                if (model != null)
                {
                    model.Color = dayMeal.Color;
                    model.Breakfast = dayMeal.Meals.Breakfast;
                    model.MorningSnack = dayMeal.Meals.MorningSnack;
                    model.Lunch = dayMeal.Meals.Lunch;
                    model.AfternoonSnack = dayMeal.Meals.AfternoonSnack;
                    model.Dinner = dayMeal.Meals.Dinner;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar dieta: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error loading diet: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveDiet()
    {
        try
        {
            if (!ValidateInput()) return;

            IsSaving = true;
            ErrorMessage = null;

            var dayMealsDto = DayMeals.Select(d => new DayMealDto
            {
                Day = d.Day,
                Color = d.Color,
                Breakfast = d.Breakfast ?? string.Empty,
                MorningSnack = d.MorningSnack ?? string.Empty,
                Lunch = d.Lunch ?? string.Empty,
                AfternoonSnack = d.AfternoonSnack ?? string.Empty,
                Dinner = d.Dinner ?? string.Empty
            }).ToList();

            if (IsEditMode && DietId.HasValue)
            {
                var updateDto = new UpdateDietDto
                {
                    Title = Title,
                    Description = Description,
                    Type = SelectedType,
                    DayMeals = dayMealsDto
                };

                var (ok, error) = await _dietService.UpdateDietAsync(DietId.Value, updateDto);
                if (ok)
                {
                    await Shell.Current.DisplayAlert("Sucesso", "Dieta atualizada com sucesso!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = error ?? "Erro ao atualizar dieta";
                }
            }
            else
            {
                var createDto = new CreateDietDto
                {
                    Title = Title,
                    Description = Description,
                    Type = SelectedType,
                    DayMeals = dayMealsDto
                };

                var (ok, error, _) = await _dietService.CreateDietAsync(createDto);
                if (ok)
                {
                    await Shell.Current.DisplayAlert("Sucesso", "Dieta criada com sucesso!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = error ?? "Erro ao criar dieta";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao salvar dieta: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error saving diet: {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void ToggleDayExpansion(DayMeal dayMeal)
    {
        dayMeal.IsExpanded = !dayMeal.IsExpanded;
    }

    [RelayCommand]
    private async Task Cancel()
    {
        var hasChanges = !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(Description) ||
                        DayMeals.Any(d => !string.IsNullOrEmpty(d.Breakfast) || !string.IsNullOrEmpty(d.MorningSnack) ||
                                         !string.IsNullOrEmpty(d.Lunch) || !string.IsNullOrEmpty(d.AfternoonSnack) ||
                                         !string.IsNullOrEmpty(d.Dinner));

        if (hasChanges)
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Descartar Alterações",
                "Você tem alterações não salvas. Deseja realmente sair?",
                "Sim, descartar",
                "Continuar editando"
            );

            if (!confirm) return;
        }

        await Shell.Current.GoToAsync("..");
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "O título é obrigatório";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            ErrorMessage = "A descrição é obrigatória";
            return false;
        }

        // Verificar se pelo menos um dia tem uma refeição
        var hasAnyMeal = DayMeals.Any(d =>
            !string.IsNullOrWhiteSpace(d.Breakfast) ||
            !string.IsNullOrWhiteSpace(d.MorningSnack) ||
            !string.IsNullOrWhiteSpace(d.Lunch) ||
            !string.IsNullOrWhiteSpace(d.AfternoonSnack) ||
            !string.IsNullOrWhiteSpace(d.Dinner));

        if (!hasAnyMeal)
        {
            ErrorMessage = "Por favor, adicione pelo menos uma refeição";
            return false;
        }

        return true;
    }
}



public class DietTypeOption
{
    public DietType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
