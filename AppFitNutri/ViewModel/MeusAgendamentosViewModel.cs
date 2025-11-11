using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Models;
using AppFitNutri.Services;

namespace AppFitNutri.ViewModel;

public class MeusAgendamentosViewModel : INotifyPropertyChanged
{
    private readonly IAgendamentoService _service;
    private bool _isLoading;
    private bool _isRefreshing;

    public ObservableCollection<AgendamentoItem> Itens { get; } = new();

    public ICommand CarregarCommand { get; }
    public ICommand AtualizarCommand { get; }
    public ICommand CancelarCommand { get; }

    public MeusAgendamentosViewModel(IAgendamentoService service)
    {
        _service = service;
        CarregarCommand = new Command(async () => await Carregar());
        AtualizarCommand = new Command(async () => await Atualizar());
        CancelarCommand = new Command<AgendamentoItem>(async item => await Cancelar(item));
    }

    public async Task InicializarAsync()
    {
        await Carregar();
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set { _isRefreshing = value; OnPropertyChanged(); }
    }

    private async Task Carregar()
    {
        try
        {
            IsLoading = true;
            Itens.Clear();
            var items = await _service.GetMeusAgendamentosAsync();
            foreach (var a in items)
            {
                Itens.Add(new AgendamentoItem
                {
                    Id = a.Id,
                    ProfissionalId = a.ProfissionalId,
                    Data = a.Data,
                    Hora = a.Hora,
                    DuracaoMinutos = a.DuracaoMinutos,
                    Status = a.Status,
                    ProfissionalNome = a.ProfissionalNome,
                    ProfissionalPerfil = a.ProfissionalPerfil
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Falha ao carregar agendamentos: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task Atualizar()
    {
        try
        {
            IsRefreshing = true;
            Itens.Clear();
            var items = await _service.GetMeusAgendamentosAsync();
            foreach (var a in items)
            {
                Itens.Add(new AgendamentoItem
                {
                    Id = a.Id,
                    ProfissionalId = a.ProfissionalId,
                    Data = a.Data,
                    Hora = a.Hora,
                    DuracaoMinutos = a.DuracaoMinutos,
                    Status = a.Status,
                    ProfissionalNome = a.ProfissionalNome,
                    ProfissionalPerfil = a.ProfissionalPerfil
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Falha ao atualizar agendamentos: {ex.Message}", "OK");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task Cancelar(AgendamentoItem? item)
    {
        if (item == null) return;
        if (!item.PodeCancelar)
        {
            await Shell.Current.DisplayAlert("Atenção", "Este agendamento já está cancelado.", "OK");
            return;
        }

        var mensagem = $"Deseja cancelar o agendamento de {item.DataTexto} às {item.HoraTexto}?";
        var ok = await Shell.Current.DisplayAlert("Cancelar", mensagem, "Sim", "Não");
        if (!ok) return;

        var (success, error) = await _service.DeletarAgendamentoAsync(item.Id);
        if (!success)
        {
            await Shell.Current.DisplayAlert("Erro", $"Não foi possível cancelar. {error}", "OK");
            return;
        }

        Itens.Remove(item);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
