using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AppFitNutri.ViewModel;

public class WorkoutChoiceViewModel : INotifyPropertyChanged
{
    public WorkoutChoiceViewModel()
    {
        GymCommand = new Command(OnSelectGym);
        HomeCommand = new Command(OnSelectHome);
        BackCommand = new Command(OnBack);
    }

    public ICommand GymCommand { get; }
    public ICommand HomeCommand { get; }
    public ICommand BackCommand { get; }

    private async void OnSelectGym()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(Views.GymWorkoutPage));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnSelectGym: {ex.Message}");
        }
    }

    private async void OnSelectHome()
    {
        try
        {
            // TODO: Navegar para página de treinos em casa
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage != null)
                await currentPage.DisplayAlert("Em Casa", "Treino em casa", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnSelectHome: {ex.Message}");
        }
    }

    private async void OnBack()
    {
        try
        {
            // Voltar para a página anterior
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnBack: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

