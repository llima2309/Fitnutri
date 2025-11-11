using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Views;

namespace AppFitNutri.ViewModel;

public class HomeNutricionistaViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public HomeNutricionistaViewModel()
    {
        AgendamentosCommand = new Command(OnAgendamentos);
        PacientesCommand = new Command(OnPacientes);
        DietasCommand = new Command(OnDietas);
        PerfilCommand = new Command(OnPerfil);
    }

    public ICommand AgendamentosCommand { get; }
    public ICommand PacientesCommand { get; }
    public ICommand DietasCommand { get; }
    public ICommand PerfilCommand { get; }

    private async void OnAgendamentos()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(AgendamentosProfissionalPage));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro em OnAgendamentos: {ex.Message}");
            await ShowErrorAsync("Erro ao abrir agendamentos");
        }
    }

    private async void OnPacientes()
    {
        try
        {
            // TODO: Criar página de lista de pacientes do nutricionista
            await Shell.Current.DisplayAlert("Pacientes", "Funcionalidade em desenvolvimento", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro em OnPacientes: {ex.Message}");
        }
    }

    private async void OnDietas()
    {
        try
        {
            // TODO: Criar página de gerenciamento de dietas do nutricionista
            await Shell.Current.DisplayAlert("Dietas", "Funcionalidade em desenvolvimento", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro em OnDietas: {ex.Message}");
        }
    }

    private async void OnPerfil()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(PerfilPage));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro em OnPerfil: {ex.Message}");
            await ShowErrorAsync("Erro ao abrir perfil");
        }
    }

    private async Task ShowErrorAsync(string message)
    {
        var currentPage = Shell.Current.CurrentPage;
        if (currentPage != null)
            await currentPage.DisplayAlert("Erro", message, "OK");
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

