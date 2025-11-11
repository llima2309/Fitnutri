namespace AppFitNutri.Models;

public class Meal
{
    public string Breakfast { get; set; } = string.Empty;
    public string MorningSnack { get; set; } = string.Empty;
    public string Lunch { get; set; } = string.Empty;
    public string AfternoonSnack { get; set; } = string.Empty;
    public string Dinner { get; set; } = string.Empty;
}

public class DayMeal
{
    public string Day { get; set; } = string.Empty;
    public string DayLabel { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Breakfast { get; set; } = string.Empty;
    public string MorningSnack { get; set; } = string.Empty;
    public string Lunch { get; set; } = string.Empty;
    public string AfternoonSnack { get; set; } = string.Empty;
    public string Dinner { get; set; } = string.Empty;
    public Meal Meals { get; set; } = new Meal();
    public bool IsExpanded { get; set; }
    
    public bool HasMeals => !string.IsNullOrWhiteSpace(Breakfast) ||
                           !string.IsNullOrWhiteSpace(MorningSnack) ||
                           !string.IsNullOrWhiteSpace(Lunch) ||
                           !string.IsNullOrWhiteSpace(AfternoonSnack) ||
                           !string.IsNullOrWhiteSpace(Dinner);
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

