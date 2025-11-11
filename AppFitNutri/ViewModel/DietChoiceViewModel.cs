using AppFitNutri.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppFitNutri.ViewModel;

public partial class DietChoiceViewModel : ObservableObject
{
    public DietChoiceViewModel()
    {
    }

    [RelayCommand]
    private async Task Keto()
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(Views.DietDetailPage)}?dietType={DietType.Keto}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Keto: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task LowCarb()
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(Views.DietDetailPage)}?dietType={DietType.LowCarb}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in LowCarb: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Vegan()
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(Views.DietDetailPage)}?dietType={DietType.Vegan}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Vegan: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Celiac()
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(Views.DietDetailPage)}?dietType={DietType.Celiac}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Celiac: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Vegetarian()
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(Views.DietDetailPage)}?dietType={DietType.Vegetarian}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Vegetarian: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"Error in GoBack: {ex.Message}");
        }
    }
}
