using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Views;

namespace AppFitNutri.ViewModel;

public class AgendamentosViewModel : INotifyPropertyChanged
{
    public AgendamentosViewModel()
    {
        SelecionarNutricionistaCommand = new Command(OnSelecionarNutricionista);
        SelecionarPersonalTrainerCommand = new Command(OnSelecionarPersonalTrainer);
        SelecionarMeusAgendamentosCommand = new Command(OnSelecionarMeusAgendamentos);
    }

    public ICommand SelecionarNutricionistaCommand { get; }
    public ICommand SelecionarPersonalTrainerCommand { get; }
    public ICommand SelecionarMeusAgendamentosCommand { get; }

    private async void OnSelecionarNutricionista()
    {
        try
        {
            // Navegação para a página de lista de profissionais passando o tipo Nutricionista (2)
            var parameters = new Dictionary<string, object>
            {
                { "TipoProfissional", 2 },
                { "NomeTipo", "Nutricionista" }
            };
            
            await Shell.Current.GoToAsync($"{nameof(ListaProfissionaisPage)}", parameters);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao navegar: {ex.Message}", "OK");
        }
    }

    private async void OnSelecionarPersonalTrainer()
    {
        try
        {
            // Navegação para a página de lista de profissionais passando o tipo Personal Trainer (3)
            var parameters = new Dictionary<string, object>
            {
                { "TipoProfissional", 3 },
                { "NomeTipo", "Personal Trainer" }
            };
            
            await Shell.Current.GoToAsync($"{nameof(ListaProfissionaisPage)}", parameters);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao navegar: {ex.Message}", "OK");
        }
    }

    private async void OnSelecionarMeusAgendamentos()
    {
        try
        {
            // Navegação para a página de Meus Agendamentos via rota
            await Shell.Current.GoToAsync("MeusAgendamentosPage");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao navegar: {ex.Message}", "OK");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
