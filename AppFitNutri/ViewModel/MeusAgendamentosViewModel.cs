using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Models;
using AppFitNutri.Services;
using AppFitNutri.Core.Services;

namespace AppFitNutri.ViewModel;

public class MeusAgendamentosViewModel : INotifyPropertyChanged
{
    private readonly IAgendamentoService _service;
    private readonly IVideoCallService _videoCallService;
    private readonly ITokenStore _tokenStore;
    private bool _isLoading;
    private bool _isRefreshing;

    public ObservableCollection<AgendamentoItem> Itens { get; } = new();

    public ICommand CarregarCommand { get; }
    public ICommand AtualizarCommand { get; }
    public ICommand CancelarCommand { get; }
    public ICommand IniciarChamadaCommand { get; }

    public MeusAgendamentosViewModel(
        IAgendamentoService service,
        IVideoCallService videoCallService,
        ITokenStore tokenStore)
    {
        _service = service;
        _videoCallService = videoCallService;
        _tokenStore = tokenStore;
        CarregarCommand = new Command(async () => await Carregar());
        AtualizarCommand = new Command(async () => await Atualizar());
        CancelarCommand = new Command<AgendamentoItem>(async item => await Cancelar(item));
        IniciarChamadaCommand = new Command<AgendamentoItem>(async item => await IniciarVideoChamadaAsync(item));
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

    private async Task IniciarVideoChamadaAsync(AgendamentoItem? item)
    {
        if (item == null) return;

        try
        {
            IsLoading = true;

            // 1. Verificar se já existe uma chamada ativa
            var status = await _videoCallService.GetStatusChamadaAsync(item.Id);
            
            VideoCallResponse? callResponse;
            
            if (status?.IsActive == true)
            {
                // Chamada já existe, apenas conectar
                System.Diagnostics.Debug.WriteLine("Chamada já ativa, conectando...");
                
                callResponse = new VideoCallResponse(
                    item.Id,
                    status.CallStartedAt?.ToString("yyyyMMddHHmmss") ?? "existing",
                    status.CallStartedAt ?? DateTime.Now,
                    "/videocall"
                );
            }
            else
            {
                // 2. Iniciar nova chamada
                callResponse = await _videoCallService.IniciarChamadaAsync(item.Id);
                
                if (callResponse == null)
                {
                    await Shell.Current.DisplayAlert("Erro", "Não foi possível iniciar a videochamada. Verifique se o agendamento está confirmado.", "OK");
                    return;
                }
            }

            IsLoading = false;

            // 3. Obter dados do usuário
            var token = await _tokenStore.GetAsync();
            var userId = await GetCurrentUserIdAsync();
            
            if (string.IsNullOrEmpty(userId))
            {
                await Shell.Current.DisplayAlert("Erro", "Erro ao obter ID do usuário", "OK");
                return;
            }

            // 4. Navegar para a página de videochamada
            var videoCallPage = new Views.VideoCallPage(
                callResponse.AgendamentoId,
                callResponse.CallToken,
                callResponse.HubUrl,
                token ?? "",
                userId,
                "paciente"
            );

            await Shell.Current.Navigation.PushModalAsync(videoCallPage);
        }
        catch (Exception ex)
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine($"Erro ao iniciar videochamada: {ex.Message}");
            await Shell.Current.DisplayAlert("Erro", $"Erro ao iniciar videochamada: {ex.Message}", "OK");
        }
    }

    private async Task<string?> GetCurrentUserIdAsync()
    {
        try
        {
            var token = await _tokenStore.GetAsync();
            if (string.IsNullOrEmpty(token)) return null;

            // Decodificar o JWT para pegar o sub (user ID)
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1];
            var paddedPayload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var bytes = Convert.FromBase64String(paddedPayload);
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            
            var jsonDoc = System.Text.Json.JsonDocument.Parse(json);
            if (jsonDoc.RootElement.TryGetProperty("sub", out var subElement))
            {
                return subElement.GetString();
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao obter user ID: {ex.Message}");
            return null;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
