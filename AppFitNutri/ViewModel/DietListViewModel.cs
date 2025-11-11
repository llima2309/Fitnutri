using System.Collections.ObjectModel;
using AppFitNutri.Models;
using AppFitNutri.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppFitNutri.ViewModel;

public partial class DietListViewModel : ObservableObject
{
    private readonly IDietService _dietService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEmpty;

    [ObservableProperty]
    private string? errorMessage;

    public ObservableCollection<DietSummaryDto> Diets { get; } = new();

    public DietListViewModel(IDietService dietService)
    {
        _dietService = dietService;
    }

    [RelayCommand]
    private async Task LoadDiets()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            Diets.Clear();

            var diets = await _dietService.GetMyDietsAsync();
            
            foreach (var diet in diets)
            {
                Diets.Add(diet);
            }

            IsEmpty = Diets.Count == 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar dietas: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error loading diets: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CreateDiet()
    {
        try
        {
            await Shell.Current.GoToAsync("CreateEditDietPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to create diet: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task EditDiet(Guid dietId)
    {
        try
        {
            await Shell.Current.GoToAsync($"CreateEditDietPage?dietId={dietId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to edit diet: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ViewDiet(Guid dietId)
    {
        try
        {
            // For now, navigate to edit page as view - can create dedicated view page later
            await Shell.Current.GoToAsync($"CreateEditDietPage?dietId={dietId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to diet details: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteDiet(DietSummaryDto diet)
    {
        try
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Confirmar Exclusão",
                $"Tem certeza que deseja excluir a dieta '{diet.Title}'?",
                "Sim",
                "Não"
            );

            if (!confirm) return;

            IsLoading = true;
            var (ok, error) = await _dietService.DeleteDietAsync(diet.Id);

            if (ok)
            {
                Diets.Remove(diet);
                IsEmpty = Diets.Count == 0;
                await Shell.Current.DisplayAlert("Sucesso", "Dieta excluída com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Erro", error ?? "Erro ao excluir dieta", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao excluir dieta: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Error deleting diet: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GoBack()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error going back: {ex.Message}");
        }
    }

    public string GetDietTypeLabel(DietType type) => type switch
    {
        DietType.Keto => "Keto",
        DietType.LowCarb => "Low Carb",
        DietType.Vegan => "Vegana",
        DietType.Celiac => "Celíaca",
        DietType.Vegetarian => "Vegetariana",
        _ => type.ToString()
    };
}

