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
        if (TipoProfissional == 0) 
        {
            System.Diagnostics.Debug.WriteLine("TipoProfissional é 0, não carregando");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"Iniciando carregamento para tipo: {TipoProfissional}");
        IsLoading = true;
        
        try
        {
            var profissionais = await _profissionaisService.GetProfissionaisByTipoAsync(TipoProfissional);
            System.Diagnostics.Debug.WriteLine($"Recebidos {profissionais?.Count ?? 0} profissionais da API");
            
            // Atualizar na UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                System.Diagnostics.Debug.WriteLine($"Limpando collection. Continha {Profissionais.Count} itens antes");
                Profissionais.Clear();
                
                if (profissionais != null)
                {
                    foreach (var profissional in profissionais)
                    {
                        System.Diagnostics.Debug.WriteLine($"Adicionando: {profissional.NomeCompleto}");
                        Profissionais.Add(profissional);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Collection agora tem {Profissionais.Count} itens");
                
                // Forçar notificação de mudança na propriedade
                OnPropertyChanged(nameof(Profissionais));
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar profissionais: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Erro",
                        $"Erro ao carregar profissionais: {ex.Message}", "OK");
                }
            });
        }
        finally
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine("Carregamento finalizado");
        }
    }

    private async void OnSelecionarProfissional(Profissional profissional)
    {
        if (profissional == null) return;

        try
        {
            // Navegação para a página de agendamento, passando o objeto Profissional como parâmetro
            var parameters = new Dictionary<string, object>
            {
                { "Profissional", profissional }
            };

            // Usar nome de rota (string) para evitar dependência de símbolo em tempo de compilação
            await Shell.Current.GoToAsync("AgendamentoPage", parameters);
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
