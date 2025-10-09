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
        LogoutCommand = new Command(OnLogout);
        AgendamentosCommand = new Command(OnAgendamentos);
        VideoChamadaCommand = new Command(OnVideoChamada);
        VideoAulasCommand = new Command(OnVideoAulas);
        TreinosCommand = new Command(OnTreinos);
        DietaCommand = new Command(OnDieta);
        PerfilCommand = new Command(OnPerfil);
    }

    public ICommand LogoutCommand { get; }
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
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Erro", "Erro ao abrir agendamentos", "OK");
        }
    }

    private async void OnVideoChamada()
    {
        try
        {
            // TODO: Implementar navegação para página de video chamada
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Video Chamada", "Funcionalidade em desenvolvimento", "OK");
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
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Video Aulas", "Funcionalidade em desenvolvimento", "OK");
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
            // TODO: Implementar navegação para página de treinos
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Treinos", "Funcionalidade em desenvolvimento", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnTreinos: {ex.Message}");
        }
    }

    private async void OnDieta()
    {
        try
        {
            // TODO: Implementar navegação para página de dieta
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Dieta", "Funcionalidade em desenvolvimento", "OK");
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
            // TODO: Implementar navegação para página de perfil
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Perfil", "Funcionalidade em desenvolvimento", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnPerfil: {ex.Message}");
        }
    }

    private async void OnLogout()
    {
        try
        {
            // Implementar logout se necessário
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnLogout: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
