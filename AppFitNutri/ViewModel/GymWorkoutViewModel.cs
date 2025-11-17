using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Models;
using AppFitNutri.Services;
using AppFitNutri.Views;

namespace AppFitNutri.ViewModel;

public class GymWorkoutViewModel : INotifyPropertyChanged
{
    private bool _isLoading;
    
    public ObservableCollection<DayWorkout> WorkoutPlan { get; set; }
    
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
    
    public ICommand BackCommand { get; }
    public ICommand ToggleDayCommand { get; }
    public ICommand OpenVideoCommand { get; }

    public GymWorkoutViewModel()
    {
        BackCommand = new Command(OnBack);
        ToggleDayCommand = new Command<DayWorkout>(OnToggleDay);
        OpenVideoCommand = new Command<Exercise>(OnOpenVideo);
        
        WorkoutPlan = new ObservableCollection<DayWorkout>();
        
        // Carrega os dados de forma assíncrona
        _ = LoadWorkoutDataAsync();
    }

    private async Task LoadWorkoutDataAsync()
    {
        IsLoading = true;
        
        try
        {
            // Mostra o modal de loading
            await LoadingService.ShowLoadingAsync();
            
            // Simula carregamento assíncrono para não bloquear a UI
            await Task.Run(async () =>
            {
                // Adiciona um delay para simular carregamento de dados
                await Task.Delay(1500);
                
                var workouts = GetWorkoutData();
                
                // Adiciona os dados na UI thread
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
            // Esconde o modal de loading
            await LoadingService.HideLoadingAsync();
        }
    }

    private List<DayWorkout> GetWorkoutData()
    {
        return new List<DayWorkout>
        {
            new DayWorkout
            {
                Day = "SEG",
                Title = "PEITO",
                GradientStart = "#3B82F6",
                GradientEnd = "#2563EB",
                IsExpanded = true,
                Exercises = new ObservableCollection<Exercise>
                {
                    new Exercise { Name = "Supino reto com halteres ou barra (maioria com halteres)", Sets = "4", Reps = "10 a 8" },
                    new Exercise { Name = "Supino inclinado no smith/máquina", Sets = "4", Reps = "10 a 8" },
                    new Exercise { Name = "Crucifixo reto com halteres ou cabo em pé", Sets = "3", Reps = "12 a 10" },
                    new Exercise { Name = "Crossover no cabo de cima pra baixo na polia", Sets = "3", Reps = "12 a 10" },
                    new Exercise { Name = "Mergulho em barras paralelas (apoio)", Sets = "3", Reps = "até a falha" }
                }
            },
            new DayWorkout
            {
                Day = "TER",
                Title = "COSTAS",
                GradientStart = "#10B981",
                GradientEnd = "#059669",
                IsExpanded = false,
                Exercises = new ObservableCollection<Exercise>
                {
                    new Exercise { Name = "Puxada frente", Sets = "4", Reps = "10 a 8" },
                    new Exercise { Name = "Remada unilateral na máquina (ou halter)", Sets = "4", Reps = "10 a 8" },
                    new Exercise { Name = "Remada cavalinho (T-bar)", Sets = "4", Reps = "10 a 8" },
                    new Exercise { Name = "Remada baixa (triângulo ou barra V)", Sets = "3", Reps = "12 a 10" },
                    new Exercise { Name = "Pullover na polia", Sets = "3", Reps = "12 a 10" }
                }
            },
            new DayWorkout
            {
                Day = "QUA",
                Title = "ALONGAMENTO",
                GradientStart = "#A855F7",
                GradientEnd = "#9333EA",
                IsExpanded = false,
                Exercises = new ObservableCollection<Exercise>
                {
                    new Exercise { Name = "Alongamento de isquiotibiais em pé (uma perna apoiada)", Sets = "2-3", Reps = "20-30s" },
                    new Exercise { Name = "Posição de \"pombo\" (glúteos e quadril)", Sets = "2-3", Reps = "20-30s" },
                    new Exercise { Name = "Alongamento de quadríceps ajoelhado unilateral", Sets = "2-3", Reps = "20-30s" },
                    new Exercise { Name = "Mobilidade de tornozelo contra parede", Sets = "2-3", Reps = "20-30s" },
                    new Exercise { Name = "Alongamento de peitoral com faixa elástica ou parede", Sets = "2-3", Reps = "20-30s" },
                    new Exercise { Name = "Alongamento de grande dorsal pendurado na barra", Sets = "2-3", Reps = "20-30s" },
                    new Exercise { Name = "Posição de Ala", Sets = "2-3", Reps = "20-30s" },
                    new Exercise { Name = "Alongamento de deltoide posterior (braço cruzado)", Sets = "2-3", Reps = "20-30s" },
                    new Exercise { Name = "Alongamento de tríceps (cotovelo acima da cabeça)", Sets = "2-3", Reps = "20-30s" },
                    new Exercise { Name = "Mobilidade de ombro com elástico", Sets = "2-3", Reps = "20-30s" }
                }
            },
            new DayWorkout
            {
                Day = "QUI",
                Title = "PERNA + GLÚTEOS",
                GradientStart = "#F97316",
                GradientEnd = "#EA580C",
                IsExpanded = false,
                Exercises = new ObservableCollection<Exercise>
                {
                    new Exercise { Name = "Cadeira extensora", Sets = "4", Reps = "12 (4ª série até a falha)" },
                    new Exercise { Name = "Leg press", Sets = "4", Reps = "10 a 8" },
                    new Exercise { Name = "Front squat barra guiada ou halter", Sets = "3", Reps = "12 a 10" },
                    new Exercise { Name = "Mesa flexora", Sets = "3", Reps = "12 a 10" },
                    new Exercise { Name = "Cadeira abdutora", Sets = "3", Reps = "12 a 10" },
                    new Exercise { Name = "Cadeira adutora", Sets = "3", Reps = "12 a 10" }
                }
            },
            new DayWorkout
            {
                Day = "SEX",
                Title = "OMBROS",
                GradientStart = "#EF4444",
                GradientEnd = "#DC2626",
                IsExpanded = false,
                Exercises = new ObservableCollection<Exercise>
                {
                    new Exercise { Name = "Desenvolvimento com barra", Sets = "4", Reps = "12 a 10" },
                    new Exercise { Name = "Elevação lateral + isometria em cima por 3 segundos", Sets = "3", Reps = "12 a 10" },
                    new Exercise { Name = "Elevação frontal + isometria em cima por 3 segundos", Sets = "3", Reps = "12 a 10" },
                    new Exercise { Name = "Peck Deck invertido", Sets = "3", Reps = "12 a 10" },
                    new Exercise { Name = "Encolhimento", Sets = "3", Reps = "12 a 10" }
                }
            },
            new DayWorkout
            {
                Day = "SÁB",
                Title = "BRAÇOS",
                GradientStart = "#6366F1",
                GradientEnd = "#4F46E5",
                IsExpanded = false,
                Exercises = new ObservableCollection<Exercise>
                {
                    new Exercise { Name = "Supersérie 1: Rosca direta na polia (barra reta)", Sets = "4", Reps = "12 a 10" },
                    new Exercise { Name = "Supersérie 1: Tríceps testa com corda na polia", Sets = "4", Reps = "12 a 10" },
                    new Exercise { Name = "Supersérie 2: Rosca unilateral concentrada", Sets = "3", Reps = "12 a 10" },
                    new Exercise { Name = "Supersérie 2: Tríceps coice unilateral na polia", Sets = "3", Reps = "12 a 10" },
                    new Exercise { Name = "Supersérie 3: Rosca scott (banco ou máquina)", Sets = "3", Reps = "12 a 10" },
                    new Exercise { Name = "Supersérie 3: Tríceps banco ou máquina", Sets = "3", Reps = "12 a 10" }
                }
            },
            new DayWorkout
            {
                Day = "DOM",
                Title = "DESCANSO",
                GradientStart = "#64748B",
                GradientEnd = "#475569",
                IsExpanded = false,
                Exercises = new ObservableCollection<Exercise>
                {
                    new Exercise { Name = "Dia de recuperação e descanso", Sets = "-", Reps = "-" }
                }
            }
        };
    }

    private async void OnBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    private void OnToggleDay(DayWorkout dayWorkout)
    {
        dayWorkout.IsExpanded = !dayWorkout.IsExpanded;
    }

    private async void OnOpenVideo(Exercise exercise)
    {
        var viewModel = new ExerciseVideoViewModel(exercise, null);
        var modal = new ExerciseVideoModal(viewModel);
        await Shell.Current.Navigation.PushModalAsync(modal);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

