using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AppFitNutri.Models;

public class DayWorkout : INotifyPropertyChanged
{
    private bool _isExpanded;

    public string Day { get; set; }
    public string Title { get; set; }
    public string GradientStart { get; set; }
    public string GradientEnd { get; set; }
    public ObservableCollection<Exercise> Exercises { get; set; }

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

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
