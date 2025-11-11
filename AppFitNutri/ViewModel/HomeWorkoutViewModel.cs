using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Models;
using AppFitNutri.Services;

namespace AppFitNutri.ViewModel;

public class HomeWorkoutViewModel : INotifyPropertyChanged
{
    private HomeWorkout? _selectedWorkout;
    private bool _isLoading;

    public ObservableCollection<HomeWorkout> Workouts { get; set; } = new();

    public HomeWorkout? SelectedWorkout
    {
        get => _selectedWorkout;
        set
        {
            if (_selectedWorkout != value)
            {
                _selectedWorkout = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand ToggleDayCommand { get; }

    public HomeWorkoutViewModel()
    {
        ToggleDayCommand = new Command<HomeWorkout>(OnToggleDay);
        _ = LoadWorkoutsAsync();
    }

    private async Task LoadWorkoutsAsync()
    {
        IsLoading = true;
        try
        {
            // Mostra o modal de loading
            await LoadingService.ShowLoadingAsync();
            
            await Task.Run(async () =>
            {
                // Adiciona um delay para simular carregamento de dados
                await Task.Delay(1500);
                
                var workouts = GetWorkouts();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var w in workouts)
                        Workouts.Add(w);
                });
            });
        }
        finally
        {
            IsLoading = false;
            // Esconde o modal de loading
            await LoadingService.HideLoadingAsync();
        }
    }

    private IEnumerable<HomeWorkout> GetWorkouts()
    {
        var workouts = new[]
        {
            new HomeWorkout
            {
                Day = "SEG",
                Title = "TREINO 1",
                BackgroundColor = "#3B82F6",
                GradientStart = "#3B82F6",
                GradientEnd = "#2563EB",
                IsExpanded = true,
                Sections = new ObservableCollection<WorkoutSection>
                {
                    new()
                    {
                        Title = "Aquecimento",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new() { Name = "Skipping (corrida no lugar)", Details = "4x 30\"/7\"" },
                            new() { Name = "Agachamento", Details = "4x 30\"/7\"" },
                            new() { Name = "Prancha", Details = "4x 30\"/7\"" }
                        }
                    },
                    new()
                    {
                        Title = "Circuito de Cardio",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new() { Name = "Agachamento com salto", Details = "2x seguidas 30\"/7\" • Intervalo 1' • 3x tudo" },
                            new() { Name = "Abdominal tesoura", Details = "2x seguidas 30\"/7\" • Intervalo 1' • 3x tudo" },
                            new() { Name = "Corrida F/C (encostar no chão)", Details = "2x seguidas 30\"/7\" • Intervalo 1' • 3x tudo" },
                            new() { Name = "Abdominal canoa isométrico", Details = "2x seguidas 30\"/7\" • Intervalo 1' • 3x tudo" }
                        }
                    }
                }
            },
            new HomeWorkout
            {
                Day = "TER",
                Title = "TREINO 2",
                BackgroundColor = "#10B981",
                GradientStart = "#10B981",
                GradientEnd = "#059669",
                Sections = new ObservableCollection<WorkoutSection>
                {
                    new()
                    {
                        Title = "Aquecimento",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new() { Name = "Hopserlauf (Hop)", Details = "3x 30\"/7\"" },
                            new() { Name = "Afundo alternado", Details = "3x 30\"/7\"" },
                            new() { Name = "Remada alta leve", Details = "3x 30\"/7\"" },
                            new() { Name = "Prancha tocando ombro + rotação de tronco", Details = "3x 30\"/7\"" }
                        }
                    },
                    new()
                    {
                        Title = "Preparo Muscular",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new() { Name = "Afundo unilateral D/E", Details = "4 a 6x de 15 repetições" },
                            new() { Name = "Flexão de braço (fechada para quem conseguir)", Details = "4 a 6x de 15 repetições" },
                            new() { Name = "Remada alta", Details = "4 a 6x de 15 repetições" }
                        }
                    }
                }
            },
            new HomeWorkout
            {
                Day = "QUA",
                Title = "TREINO 3",
                BackgroundColor = "#8B5CF6",
                GradientStart = "#A855F7",
                GradientEnd = "#9333EA",
                Sections = new ObservableCollection<WorkoutSection>
                {
                    new()
                    {
                        Title = "Alongamentos",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new() { Name = "Giro de ombro com cabo de vassoura", Details = "3 a 4x de 10 repetições" },
                            new() { Name = "Alongamento de tornozelo na parede D/E", Details = "3 a 4x de 10 repetições (cada lado)" },
                            new() { Name = "Gato/Cachorro", Details = "3 a 4x de 10 repetições" },
                            new() { Name = "Alongamento posterior de coxa + iliopsoas semi ajoelhado D/E", Details = "3 a 4x de 10 repetições (cada lado)" }
                        }
                    }
                }
            },
            new HomeWorkout
            {
                Day = "QUI",
                Title = "TREINO 4",
                BackgroundColor = "#F59E0B",
                GradientStart = "#F97316",
                GradientEnd = "#EA580C",
                Sections = new ObservableCollection<WorkoutSection>
                {
                    new()
                    {
                        Title = "Aquecimento",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new() { Name = "Anfersen", Details = "5x 25\"/7\"" },
                            new() { Name = "Afundo alternado", Details = "5x 25\"/7\"" },
                            new() { Name = "Abdominal curto", Details = "5x 25\"/7\"" }
                        }
                    },
                    new()
                    {
                        Title = "Prevenção de Lesão",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new() { Name = "Salto ski equilíbrio", Details = "2x • 2 voltas seguidas • 40\"/20\" ou 10-20 reps" },
                            new() { Name = "Afundo equilíbrio D/E", Details = "2x • 2 voltas seguidas • 40\"/20\" ou 10-20 reps" },
                            new() { Name = "Rolamento", Details = "2x • 2 voltas seguidas • 40\"/20\" ou 10-20 reps" },
                            new() { Name = "Alongamento aranha alternado", Details = "2x • 2 voltas seguidas • 40\"/20\" ou 10-20 reps" }
                        }
                    }
                }
            },
            new HomeWorkout
            {
                Day = "SEX",
                Title = "TREINO 5",
                BackgroundColor = "#EC4899",
                GradientStart = "#EF4444",
                GradientEnd = "#DC2626",
                Sections = new ObservableCollection<WorkoutSection>
                {
                    new()
                    {
                        Title = "Aquecimento",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new() { Name = "Skipping", Details = "4x 30\"/7\"" },
                            new() { Name = "Afundo alternado", Details = "4x 30\"/7\"" },
                            new() { Name = "Prancha cotovelo flexionado", Details = "4x 30\"/7\"" }
                        }
                    },
                    new()
                    {
                        Title = "Circuito",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new() { Name = "Afundo alternado com salto", Details = "3x • 2 voltas seguidas • 20\"/10\"" },
                            new() { Name = "Abdominal curto cruzado", Details = "3x • 2 voltas seguidas • 20\"/10\"" },
                            new() { Name = "Polichinelo", Details = "3x • 2 voltas seguidas • 20\"/10\"" },
                            new() { Name = "Prancha cotovelo flexionado elevando braço", Details = "3x • 2 voltas seguidas • 20\"/10\"" }
                        }
                    }
                }
            },
            new HomeWorkout
            {
                Day = "SÁB",
                Title = "TREINO 6",
                BackgroundColor = "#6366F1",
                GradientStart = "#6366F1",
                GradientEnd = "#4F46E5",
                Sections = new ObservableCollection<WorkoutSection>
                {
                    new()
                    {
                        Title = "Alongamentos",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new() { Name = "Agachamento lateral D/E", Details = "3 a 4x de 10 repetições (cada lado)" },
                            new() { Name = "Agachamento em 4 D/E", Details = "3 a 4x de 10 repetições (cada lado)" },
                            new() { Name = "Alongamento de escápula (mãos na parede)", Details = "3 a 4x de 10 repetições" },
                            new() { Name = "Rotação de tronco 6 D/E", Details = "3 a 4x de 10 repetições (cada lado)" }
                        }
                    }
                }
            },
            new HomeWorkout
            {
                Day = "DOM",
                Title = "DESCANSO",
                GradientStart = "#64748B",
                GradientEnd = "#475569",
                IsExpanded = false,
                Sections = new ObservableCollection<WorkoutSection>
                {
                    new()
                    {
                        Title = "",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new Exercise { Name = "Dia de recuperação e descanso", Sets = "-", Reps = "-" }
                        }
                    }
                }
            }
        };
        return workouts;
    }

    public void SelectWorkout(HomeWorkout workout)
    {
        SelectedWorkout = workout;
    }

    private void OnToggleDay(HomeWorkout workout)
    {
        if (workout == null) return;
        workout.IsExpanded = !workout.IsExpanded;
        if (workout.IsExpanded)
            SelectedWorkout = workout; // Optional: update details view when expanded
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
