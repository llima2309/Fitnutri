using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AppFitNutri.Models;

public class HomeWorkout : INotifyPropertyChanged
{
    private bool _isExpanded;

    public string Day { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public ObservableCollection<WorkoutSection> Sections { get; set; } = new();
    public bool IsTable { get; set; }
    public TableData? TableData { get; set; }
    // Expand/collapse state with notification
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
    public string? GradientStart { get; set; }
    public string? GradientEnd { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
