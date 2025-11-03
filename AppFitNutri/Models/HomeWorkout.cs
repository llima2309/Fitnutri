using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AppFitNutri.Models;

public class HomeWorkout : INotifyPropertyChanged
{
    private bool _isSelected;

    public string Day { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string GradientStart { get; set; } = string.Empty;
    public string GradientEnd { get; set; } = string.Empty;
    public ObservableCollection<WorkoutSection> Sections { get; set; } = new();
    public bool IsTable { get; set; }
    public TableData? TableData { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
