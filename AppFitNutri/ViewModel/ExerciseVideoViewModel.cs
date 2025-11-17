using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Models;

namespace AppFitNutri.ViewModel;

public class ExerciseVideoViewModel : INotifyPropertyChanged
{
    private string _exerciseName = string.Empty;
    private string _sets = string.Empty;
    private string _reps = string.Empty;
    private string _videoUrl = string.Empty;

    public string ExerciseName
    {
        get => _exerciseName;
        set
        {
            _exerciseName = value;
            OnPropertyChanged();
        }
    }

    public string Sets
    {
        get => _sets;
        set
        {
            _sets = value;
            OnPropertyChanged();
        }
    }

    public string Reps
    {
        get => _reps;
        set
        {
            _reps = value;
            OnPropertyChanged();
        }
    }

    public string VideoUrl
    {
        get => _videoUrl;
        set
        {
            _videoUrl = value;
            OnPropertyChanged();
        }
    }

    public ICommand CloseCommand { get; }

    public ExerciseVideoViewModel(Exercise exercise, Action? onClose)
    {
        ExerciseName = exercise.Name ?? string.Empty;
        Sets = exercise.Sets ?? string.Empty;
        Reps = exercise.Reps ?? string.Empty;
        VideoUrl = exercise.VideoUrl ?? string.Empty;

        CloseCommand = new Command(async () =>
        {
            onClose?.Invoke();
            await Shell.Current.Navigation.PopModalAsync();
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

