using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Models;
using AppFitNutri.Services;

namespace AppFitNutri.ViewModel;

public class AgendamentosProfissionalViewModel : INotifyPropertyChanged
{
    private readonly IAgendamentoService _agendamentoService;
    private bool _isLoading;
    private bool _hasAgendamentos;

    public event PropertyChangedEventHandler? PropertyChanged;

    public AgendamentosProfissionalViewModel(IAgendamentoService agendamentoService)
    {
        _agendamentoService = agendamentoService;
        Agendamentos = new ObservableCollection<AgendamentoDto>();
        
        RefreshCommand = new Command(async () => await LoadAgendamentosAsync());
        ConfirmarCommand = new Command<AgendamentoDto>(async (agendamento) => await ConfirmarAgendamentoAsync(agendamento));
        CancelarCommand = new Command<AgendamentoDto>(async (agendamento) => await CancelarAgendamentoAsync(agendamento));
        
        _ = LoadAgendamentosAsync();
    }

    public ObservableCollection<AgendamentoDto> Agendamentos { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public bool HasAgendamentos
    {
        get => _hasAgendamentos;
        set
        {
            _hasAgendamentos = value;
            OnPropertyChanged();
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand ConfirmarCommand { get; }
    public ICommand CancelarCommand { get; }

    private async Task LoadAgendamentosAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            
            var agendamentos = await _agendamentoService.GetAgendamentosProfissionalAsync();
            
            Agendamentos.Clear();
            foreach (var agendamento in agendamentos)
            {
                Agendamentos.Add(agendamento);
            }

            HasAgendamentos = Agendamentos.Count > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar agendamentos: {ex.Message}");
            await ShowErrorAsync("Erro ao carregar agendamentos");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ConfirmarAgendamentoAsync(AgendamentoDto agendamento)
    {
        if (agendamento == null) return;

        try
        {
            var confirmar = await Shell.Current.DisplayAlert(
                "Confirmar Agendamento",
                $"Deseja confirmar o agendamento com {agendamento.ProfissionalNome ?? "paciente"} para {agendamento.Data:dd/MM/yyyy} às {agendamento.Hora:HH:mm}?",
                "Confirmar",
                "Cancelar");

            if (!confirmar) return;

            IsLoading = true;

            // Status 1 = Confirmado
            var (ok, error) = await _agendamentoService.AtualizarAgendamentoAsync(
                agendamento.Id,
                status: 1);

            if (ok)
            {
                await Shell.Current.DisplayAlert("Sucesso", "Agendamento confirmado com sucesso!", "OK");
                await LoadAgendamentosAsync();
            }
            else
            {
                await ShowErrorAsync($"Erro ao confirmar agendamento: {error}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao confirmar agendamento: {ex.Message}");
            await ShowErrorAsync("Erro ao confirmar agendamento");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CancelarAgendamentoAsync(AgendamentoDto agendamento)
    {
        if (agendamento == null) return;

        try
        {
            var confirmar = await Shell.Current.DisplayAlert(
                "Cancelar Agendamento",
                $"Deseja realmente cancelar o agendamento com {agendamento.ProfissionalNome ?? "paciente"}?",
                "Sim",
                "Não");

            if (!confirmar) return;

            IsLoading = true;

            // Status 2 = Cancelado
            var (ok, error) = await _agendamentoService.AtualizarAgendamentoAsync(
                agendamento.Id,
                status: 2);

            if (ok)
            {
                await Shell.Current.DisplayAlert("Sucesso", "Agendamento cancelado com sucesso!", "OK");
                await LoadAgendamentosAsync();
            }
            else
            {
                await ShowErrorAsync($"Erro ao cancelar agendamento: {error}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao cancelar agendamento: {ex.Message}");
            await ShowErrorAsync("Erro ao cancelar agendamento");
        }
        finally
        {
            IsLoading = false;
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

