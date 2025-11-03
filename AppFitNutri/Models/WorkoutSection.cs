using System.Collections.ObjectModel;

namespace AppFitNutri.Models;

public class WorkoutSection
{
    public string Title { get; set; } = string.Empty;
    public ObservableCollection<Exercise> Exercises { get; set; } = new();
}

