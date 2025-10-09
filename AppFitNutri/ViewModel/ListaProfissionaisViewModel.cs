using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Models;
using AppFitNutri.Services;

namespace AppFitNutri.ViewModel;

[QueryProperty(nameof(TipoProfissional), "TipoProfissional")]
[QueryProperty(nameof(NomeTipo), "NomeTipo")]
public class ListaProfissionaisViewModel : INotifyPropertyChanged
{
    private readonly IProfissionaisService _profissionaisService;
    private bool _isLoading;
    private int _tipoProfissional;
    private string _nomeTipo = string.Empty;

    public ListaProfissionaisViewModel(IProfissionaisService profissionaisService)
    {
        _profissionaisService = profissionaisService;
        Profissionais = new ObservableCollection<Profissional>();
        CarregarProfissionaisCommand = new Command(async () => await CarregarProfissionais());
        SelecionarProfissionalCommand = new Command<Profissional>(OnSelecionarProfissional);
    }

    public ObservableCollection<Profissional> Profissionais { get; }
    public ICommand CarregarProfissionaisCommand { get; }
    public ICommand SelecionarProfissionalCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public int TipoProfissional
    {
        get => _tipoProfissional;
        set
        {
            _tipoProfissional = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TituloPagina));
            OnPropertyChanged(nameof(SubtituloPagina));
            OnPropertyChanged(nameof(MensagemVazia));
            
            // Carregar profissionais quando o tipo for definido
            Task.Run(async () => await CarregarProfissionais());
        }
    }

    public string NomeTipo
    {
        get => _nomeTipo;
        set
        {
            _nomeTipo = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TituloPagina));
            OnPropertyChanged(nameof(SubtituloPagina));
            OnPropertyChanged(nameof(MensagemVazia));
        }
    }

    public string TituloPagina => $"{NomeTipo}s Disponíveis";

    public string SubtituloPagina => $"Selecione um {NomeTipo.ToLower()} para agendar sua consulta";

    public string MensagemVazia => $"Nenhum {NomeTipo.ToLower()} encontrado no momento.";

    private async Task CarregarProfissionais()
    {
        if (TipoProfissional == 0) return;

        IsLoading = true;
        
        try
        {
            var profissionais = await _profissionaisService.GetProfissionaisByTipoAsync(TipoProfissional);
            
            // Atualizar na UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Profissionais.Clear();
                foreach (var profissional in profissionais)
                {
                    Profissionais.Add(profissional);
                }
            });
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current?.MainPage?.DisplayAlert("Erro", 
                    $"Erro ao carregar profissionais: {ex.Message}", "OK");
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void OnSelecionarProfissional(Profissional profissional)
    {
        if (profissional == null) return;

        try
        {
            // TODO: Implementar navegação para página de detalhes do profissional ou agendamento
            await Application.Current?.MainPage?.DisplayAlert("Profissional Selecionado", 
                $"Você selecionou: {profissional.NomeCompleto}\n\nFuncionalidade de agendamento em desenvolvimento.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Erro", 
                $"Erro ao selecionar profissional: {ex.Message}", "OK");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
