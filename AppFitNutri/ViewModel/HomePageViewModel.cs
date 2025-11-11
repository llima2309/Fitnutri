using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Views;

namespace AppFitNutri.ViewModel;

public class HomePageViewModel : INotifyPropertyChanged
{
    private string _welcomeMessage = "Bem-vindo ao FitNutri!";

    public HomePageViewModel()
    {
        AgendamentosCommand = new Command(OnAgendamentos);
        VideoChamadaCommand = new Command(OnVideoChamada);
        VideoAulasCommand = new Command(OnVideoAulas);
        TreinosCommand = new Command(OnTreinos);
        DietaCommand = new Command(OnDieta);
        PerfilCommand = new Command(OnPerfil);
    }

    public ICommand AgendamentosCommand { get; }
    public ICommand VideoChamadaCommand { get; }
    public ICommand VideoAulasCommand { get; }
    public ICommand TreinosCommand { get; }
    public ICommand DietaCommand { get; }
    public ICommand PerfilCommand { get; }

    public string WelcomeMessage
    {
        get => _welcomeMessage;
        set
        {
            _welcomeMessage = value;
            OnPropertyChanged();
        }
    }

    private async void OnAgendamentos()
    {
        try
        {
            // Navegar para a página de agendamentos
            await Shell.Current.GoToAsync(nameof(AgendamentosPage));
        }
        catch (Exception ex)
        {
            // Handle exception
            System.Diagnostics.Debug.WriteLine($"Error in OnAgendamentos: {ex.Message}");
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage != null)
                await currentPage.DisplayAlert("Erro", "Erro ao abrir agendamentos", "OK");
        }
    }

    private async void OnVideoChamada()
    {
        try
        {
            // TODO: Implementar navegação para página de video chamada
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage != null)
                await currentPage.DisplayAlert("Video Chamada", "Funcionalidade em desenvolvimento", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnVideoChamada: {ex.Message}");
        }
    }

    private async void OnVideoAulas()
    {
        try
        {
            // TODO: Implementar navegação para página de video aulas
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage != null)
                await currentPage.DisplayAlert("Video Aulas", "Funcionalidade em desenvolvimento", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnVideoAulas: {ex.Message}");
        }
    }

    private async void OnTreinos()
    {
        try
        {
            // Navegar para a página de escolha de tipo de treino
            await Shell.Current.GoToAsync(nameof(WorkoutChoicePage));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnTreinos: {ex.Message}");
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage != null)
                await currentPage.DisplayAlert("Erro", "Erro ao abrir seleção de treinos", "OK");
        }
    }

    private async void OnDieta()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(DietChoiceScreen));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnDieta: {ex.Message}");
        }
    }

    private async void OnPerfil()
    {
        try
        {
            // Navegar para a página de perfil
            await Shell.Current.GoToAsync(nameof(PerfilPage));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnPerfil: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
