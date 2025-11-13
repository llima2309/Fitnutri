using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AppFitNutri.Models;

public class Meal
{
    public string Breakfast { get; set; } = string.Empty;
    public string MorningSnack { get; set; } = string.Empty;
    public string Lunch { get; set; } = string.Empty;
    public string AfternoonSnack { get; set; } = string.Empty;
    public string Dinner { get; set; } = string.Empty;
}

public class DayMeal : INotifyPropertyChanged
{
    private bool _isExpanded;
    private string _breakfast = string.Empty;
    private string _morningSnack = string.Empty;
    private string _lunch = string.Empty;
    private string _afternoonSnack = string.Empty;
    private string _dinner = string.Empty;
    
    public string Day { get; set; } = string.Empty;
    public string DayLabel { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    
    public string Breakfast 
    { 
        get => _breakfast;
        set
        {
            if (_breakfast != value)
            {
                _breakfast = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMeals));
            }
        }
    }
    
    public string MorningSnack 
    { 
        get => _morningSnack;
        set
        {
            if (_morningSnack != value)
            {
                _morningSnack = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMeals));
            }
        }
    }
    
    public string Lunch 
    { 
        get => _lunch;
        set
        {
            if (_lunch != value)
            {
                _lunch = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMeals));
            }
        }
    }
    
    public string AfternoonSnack 
    { 
        get => _afternoonSnack;
        set
        {
            if (_afternoonSnack != value)
            {
                _afternoonSnack = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMeals));
            }
        }
    }
    
    public string Dinner 
    { 
        get => _dinner;
        set
        {
            if (_dinner != value)
            {
                _dinner = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMeals));
            }
        }
    }
    
    public Meal Meals { get; set; } = new Meal();
    
    public bool IsExpanded 
    { 
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
    }
    
    public bool HasMeals => !string.IsNullOrWhiteSpace(Breakfast) ||
                           !string.IsNullOrWhiteSpace(MorningSnack) ||
                           !string.IsNullOrWhiteSpace(Lunch) ||
                           !string.IsNullOrWhiteSpace(AfternoonSnack) ||
                           !string.IsNullOrWhiteSpace(Dinner);

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum DietType
{
    Keto,
    LowCarb,
    Vegan,
    Celiac,
    Vegetarian
}

public class DietPlan
{
    public DietType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<DayMeal> DayMeals { get; set; } = new List<DayMeal>();
}

