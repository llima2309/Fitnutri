using System.Collections.ObjectModel;
using AppFitNutri.Models;
using AppFitNutri.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppFitNutri.ViewModel;

[QueryProperty(nameof(DietTypeParam), "dietType")]
public partial class DietDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private string dietTypeParam = string.Empty;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private ObservableCollection<DayMeal> dayMeals = new();

    [ObservableProperty]
    private DayMeal? selectedDay;

    [ObservableProperty]
    private string selectedDayFullName = string.Empty;

    [ObservableProperty]
    private int selectedDayNumber;

    partial void OnDietTypeParamChanged(string value)
    {
        if (Enum.TryParse<DietType>(value, out var dietType))
        {
            LoadDietPlan(dietType);
        }
    }

    partial void OnSelectedDayChanged(DayMeal? value)
    {
        if (value != null)
        {
            SelectedDayFullName = GetFullDayName(value.Day);
            SelectedDayNumber = DayMeals.IndexOf(value) + 1;
        }
    }

    private void LoadDietPlan(DietType dietType)
    {
        var dietPlan = DietDataService.GetDietPlan(dietType);
        Title = dietPlan.Title;
        Description = dietPlan.Description;
        DayMeals = new ObservableCollection<DayMeal>(dietPlan.DayMeals);
        
        // Set current day as selected by default
        var currentDayIndex = GetCurrentDayIndex();
        if (currentDayIndex < DayMeals.Count)
        {
            SelectedDay = DayMeals[currentDayIndex];
        }
    }

    private int GetCurrentDayIndex()
    {
        var dayOfWeek = DateTime.Now.DayOfWeek;
        // Convert: Monday=0, Tuesday=1, ..., Sunday=6
        return dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;
    }

    private string GetFullDayName(string shortDay)
    {
        return shortDay switch
        {
            "SEG" => "SEGUNDA-FEIRA",
            "TER" => "TERÇA-FEIRA",
            "QUA" => "QUARTA-FEIRA",
            "QUI" => "QUINTA-FEIRA",
            "SEX" => "SEXTA-FEIRA",
            "SÁB" => "SÁBADO",
            "DOM" => "DOMINGO",
            _ => string.Empty
        };
    }

    [RelayCommand]
    private void SelectDay(DayMeal day)
    {
        SelectedDay = day;
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

