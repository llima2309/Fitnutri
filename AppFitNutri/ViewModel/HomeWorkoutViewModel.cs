using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Models;

namespace AppFitNutri.ViewModel;

public class HomeWorkoutViewModel : INotifyPropertyChanged
{
    private bool _isLoading;
    private HomeWorkout? _selectedWorkout;
    
    public ObservableCollection<HomeWorkout> WorkoutPlan { get; set; }
    
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

    public HomeWorkout? SelectedWorkout
    {
        get => _selectedWorkout;
        set
        {
            if (_selectedWorkout != value)
            {
                _selectedWorkout = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedWorkout));
            }
        }
    }

    public bool HasSelectedWorkout => SelectedWorkout != null;
    
    public ICommand BackCommand { get; }
    public ICommand SelectDayCommand { get; }

    public HomeWorkoutViewModel()
    {
        BackCommand = new Command(OnBack);
        SelectDayCommand = new Command<HomeWorkout>(OnSelectDay);
        
        WorkoutPlan = new ObservableCollection<HomeWorkout>();
        
        // Carrega os dados de forma assíncrona
        _ = LoadWorkoutDataAsync();
    }

    private async Task LoadWorkoutDataAsync()
    {
        IsLoading = true;
        
        try
        {
            await Task.Run(() =>
            {
                var workouts = GetWorkoutData();
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var workout in workouts)
                    {
                        WorkoutPlan.Add(workout);
                    }
                });
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    private List<HomeWorkout> GetWorkoutData()
    {
        return new List<HomeWorkout>
        {
            new HomeWorkout
            {
                Day = "SEG",
                Title = "TREINO 1",
                GradientStart = "#3B82F6",
                GradientEnd = "#2563EB",
                IsSelected = false,
                Sections = new ObservableCollection<WorkoutSection>
                {
                    new WorkoutSection
                    {
                        Title = "Aquecimento",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new Exercise { Name = "Skipping (corrida no lugar)", Details = "4x 30\"/7\"" },
                            new Exercise { Name = "Agachamento", Details = "4x 30\"/7\"" },
                            new Exercise { Name = "Prancha", Details = "4x 30\"/7\"" }
                        }
                    },
                    new WorkoutSection
                    {
                        Title = "Circuito de Cardio",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new Exercise { Name = "Agachamento com salto", Details = "2x seguidas 30\"/7\" • Intervalo 1' • 3x tudo" },
                            new Exercise { Name = "Abdominal tesoura", Details = "2x seguidas 30\"/7\" • Intervalo 1' • 3x tudo" },
                            new Exercise { Name = "Corrida F/C (encostar no chão)", Details = "2x seguidas 30\"/7\" • Intervalo 1' • 3x tudo" },
                            new Exercise { Name = "Abdominal canoa isométrico", Details = "2x seguidas 30\"/7\" • Intervalo 1' • 3x tudo" }
                        }
                    }
                }
            },
            new HomeWorkout
            {
                Day = "TER",
                Title = "TREINO 2",
                GradientStart = "#10B981",
                GradientEnd = "#059669",
                IsSelected = false,
                Sections = new ObservableCollection<WorkoutSection>
                {
                    new WorkoutSection
                    {
                        Title = "Aquecimento",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new Exercise { Name = "Hopserlauf (Hop)", Details = "3x 30\"/7\"" },
                            new Exercise { Name = "Afundo alternado", Details = "3x 30\"/7\"" },
                            new Exercise { Name = "Remada alta leve", Details = "3x 30\"/7\"" },
                            new Exercise { Name = "Prancha tocando ombro + rotação de tronco", Details = "3x 30\"/7\"" }
                        }
                    },
                    new WorkoutSection
                    {
                        Title = "Preparo Muscular",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new Exercise { Name = "Afundo unilateral D/E", Details = "4 a 6x de 15 repetições" },
                            new Exercise { Name = "Flexão de braço (fechada para quem conseguir)", Details = "4 a 6x de 15 repetições" },
                            new Exercise { Name = "Remada alta", Details = "4 a 6x de 15 repetições" }
                        }
                    }
                }
            },
            new HomeWorkout
            {
                Day = "QUA",
                Title = "TREINO 3",
                GradientStart = "#A855F7",
                GradientEnd = "#9333EA",
                IsSelected = false,
                Sections = new ObservableCollection<WorkoutSection>
                {
                    new WorkoutSection
                    {
                        Title = "Alongamentos",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new Exercise { Name = "Giro de ombro com cabo de vassoura", Details = "3 a 4x de 10 repetições" },
                            new Exercise { Name = "Alongamento de tornozelo na parede D/E", Details = "3 a 4x de 10 repetições (cada lado)" },
                            new Exercise { Name = "Gato/Cachorro", Details = "3 a 4x de 10 repetições" },
                            new Exercise { Name = "Alongamento posterior de coxa + iliopsoas semi ajoelhado D/E", Details = "3 a 4x de 10 repetições (cada lado)" }
                        }
                    }
                }
            },
            new HomeWorkout
            {
                Day = "QUI",
                Title = "TREINO 4",
                GradientStart = "#F97316",
                GradientEnd = "#EA580C",
                IsSelected = false,
                Sections = new ObservableCollection<WorkoutSection>
                {
                    new WorkoutSection
                    {
                        Title = "Aquecimento",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new Exercise { Name = "Anfersen", Details = "5x 25\"/7\"" },
                            new Exercise { Name = "Afundo alternado", Details = "5x 25\"/7\"" },
                            new Exercise { Name = "Abdominal curto", Details = "5x 25\"/7\"" }
                        }
                    },
                    new WorkoutSection
                    {
                        Title = "Prevenção de Lesão",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new Exercise { Name = "Salto ski equilíbrio", Details = "2x • 2 voltas seguidas • 40\"/20\" ou 10-20 reps" },
                            new Exercise { Name = "Afundo equilíbrio D/E", Details = "2x • 2 voltas seguidas • 40\"/20\" ou 10-20 reps" },
                            new Exercise { Name = "Rolamento", Details = "2x • 2 voltas seguidas • 40\"/20\" ou 10-20 reps" },
                            new Exercise { Name = "Alongamento aranha alternado", Details = "2x • 2 voltas seguidas • 40\"/20\" ou 10-20 reps" }
                        }
                    }
                }
            },
            new HomeWorkout
            {
                Day = "SEX",
                Title = "TREINO 5",
                GradientStart = "#EC4899",
                GradientEnd = "#DB2777",
                IsSelected = false,
                Sections = new ObservableCollection<WorkoutSection>
                {
                    new WorkoutSection
                    {
                        Title = "Aquecimento",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new Exercise { Name = "Skipping", Details = "4x 30\"/7\"" },
                            new Exercise { Name = "Afundo alternado", Details = "4x 30\"/7\"" },
                            new Exercise { Name = "Prancha cotovelo flexionado", Details = "4x 30\"/7\"" }
                        }
                    },
                    new WorkoutSection
                    {
                        Title = "Circuito",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new Exercise { Name = "Afundo alternado com salto", Details = "3x • 2 voltas seguidas • 20\"/10\"" },
                            new Exercise { Name = "Abdominal curto cruzado", Details = "3x • 2 voltas seguidas • 20\"/10\"" },
                            new Exercise { Name = "Polichinelo", Details = "3x • 2 voltas seguidas • 20\"/10\"" },
                            new Exercise { Name = "Prancha cotovelo flexionado elevando braço", Details = "3x • 2 voltas seguidas • 20\"/10\"" }
                        }
                    }
                }
            },
            new HomeWorkout
            {
                Day = "SÁB",
                Title = "TREINO 6",
                GradientStart = "#6366F1",
                GradientEnd = "#4F46E5",
                IsSelected = false,
                Sections = new ObservableCollection<WorkoutSection>
                {
                    new WorkoutSection
                    {
                        Title = "Alongamentos",
                        Exercises = new ObservableCollection<Exercise>
                        {
                            new Exercise { Name = "Agachamento lateral D/E", Details = "3 a 4x de 10 repetições (cada lado)" },
                            new Exercise { Name = "Agachamento em 4 D/E", Details = "3 a 4x de 10 repetições (cada lado)" },
                            new Exercise { Name = "Alongamento de escápula (mãos na parede)", Details = "3 a 4x de 10 repetições" },
                            new Exercise { Name = "Rotação de tronco 6 D/E", Details = "3 a 4x de 10 repetições (cada lado)" }
                        }
                    }
                }
            },
            new HomeWorkout
            {
                Day = "DOM",
                Title = "TREINO 7 - LISTA",
                GradientStart = "#EF4444",
                GradientEnd = "#DC2626",
                IsSelected = false,
                IsTable = true,
                Sections = new ObservableCollection<WorkoutSection>(),
                TableData = new TableData
                {
                    Headers = new ObservableCollection<string> { "Exercício", "Avançado", "Intermediário", "Iniciante" },
                    Rows = new ObservableCollection<TableRow>
                    {
                        new TableRow { Exercise = "Polichinelo", Advanced = 120, Intermediate = 100, Beginner = 90 },
                        new TableRow { Exercise = "Afundo*", Advanced = 100, Intermediate = 90, Beginner = 80, Note = "*" },
                        new TableRow { Exercise = "Elevação pélvica", Advanced = 90, Intermediate = 80, Beginner = 70 },
                        new TableRow { Exercise = "Escalador", Advanced = 80, Intermediate = 70, Beginner = 60 },
                        new TableRow { Exercise = "Abdominal curto", Advanced = 70, Intermediate = 60, Beginner = 50 },
                        new TableRow { Exercise = "High pull", Advanced = 60, Intermediate = 50, Beginner = 40 },
                        new TableRow { Exercise = "Abdominal remador", Advanced = 50, Intermediate = 40, Beginner = 30 },
                        new TableRow { Exercise = "Thruster", Advanced = 40, Intermediate = 30, Beginner = 20 },
                        new TableRow { Exercise = "Abdominal borboleta", Advanced = 30, Intermediate = 20, Beginner = 10 },
                        new TableRow { Exercise = "Burpee", Advanced = 20, Intermediate = 10, Beginner = 5 }
                    }
                }
            }
        };
    }

    private async void OnBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    private void OnSelectDay(HomeWorkout workout)
    {
        // Deseleciona todos
        foreach (var w in WorkoutPlan)
        {
            w.IsSelected = false;
        }
        
        // Seleciona o clicado
        workout.IsSelected = true;
        SelectedWorkout = workout;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


