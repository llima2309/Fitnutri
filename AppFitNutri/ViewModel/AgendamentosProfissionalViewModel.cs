using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Models;
using AppFitNutri.Services;
using AppFitNutri.Core.Services;

namespace AppFitNutri.ViewModel;

public class AgendamentosProfissionalViewModel : INotifyPropertyChanged
{
    private readonly IAgendamentoService _agendamentoService;
    private readonly IVideoCallService _videoCallService;
    private readonly ITokenStore _tokenStore;
    private bool _isLoading;
    private bool _hasAgendamentos;

    public event PropertyChangedEventHandler? PropertyChanged;

    public AgendamentosProfissionalViewModel(
        IAgendamentoService agendamentoService, 
        IVideoCallService videoCallService,
        ITokenStore tokenStore)
    {
        _agendamentoService = agendamentoService;
        _videoCallService = videoCallService;
        _tokenStore = tokenStore;
        Agendamentos = new ObservableCollection<AgendamentoDto>();
        
        RefreshCommand = new Command(async () => await LoadAgendamentosAsync());
        ConfirmarCommand = new Command<AgendamentoDto>(async (agendamento) => await ConfirmarAgendamentoAsync(agendamento));
        CancelarCommand = new Command<AgendamentoDto>(async (agendamento) => await CancelarAgendamentoAsync(agendamento));
        IniciarChamadaCommand = new Command<AgendamentoDto>(async (agendamento) => await IniciarVideoChamadaAsync(agendamento));
        
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
    public ICommand IniciarChamadaCommand { get; }

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

            var (ok, error) = await _agendamentoService.ConfirmarAgendamentoAsync(agendamento.Id);

            IsLoading = false;

            if (ok)
            {
                await LoadAgendamentosAsync();
            }
            else
            {
                await ShowErrorAsync($"Erro ao confirmar agendamento: {error}");
            }
        }
        catch (Exception ex)
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine($"Erro ao confirmar agendamento: {ex.Message}");
            await ShowErrorAsync("Erro ao confirmar agendamento");
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

            var (ok, error) = await _agendamentoService.CancelarAgendamentoAsync(agendamento.Id);

            IsLoading = false;

            if (ok)
            {
                await LoadAgendamentosAsync();
            }
            else
            {
                await ShowErrorAsync($"Erro ao cancelar agendamento: {error}");
            }
        }
        catch (Exception ex)
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine($"Erro ao cancelar agendamento: {ex.Message}");
            await ShowErrorAsync("Erro ao cancelar agendamento");
        }
    }

    private async Task IniciarVideoChamadaAsync(AgendamentoDto agendamento)
    {
        if (agendamento == null) return;

        try
        {
            IsLoading = true;

            // 1. Verificar se já existe uma chamada ativa
            var status = await _videoCallService.GetStatusChamadaAsync(agendamento.Id);
            
            VideoCallResponse? callResponse;
            
            if (status?.IsActive == true)
            {
                // Chamada já existe, apenas conectar
                System.Diagnostics.Debug.WriteLine("Chamada já ativa, conectando...");
                
                // Usar o token e hubUrl existentes
                callResponse = new VideoCallResponse(
                    agendamento.Id,
                    status.CallStartedAt?.ToString("yyyyMMddHHmmss") ?? "existing",
                    status.CallStartedAt ?? DateTime.Now,
                    "/videocall"
                );
            }
            else
            {
                // 2. Iniciar nova chamada
                callResponse = await _videoCallService.IniciarChamadaAsync(agendamento.Id);
                
                if (callResponse == null)
                {
                    await ShowErrorAsync("Não foi possível iniciar a videochamada. Verifique se o agendamento está confirmado.");
                    return;
                }
            }

            IsLoading = false;

            // 3. Obter dados do usuário
            var token = await _tokenStore.GetAsync();
            var userId = await GetCurrentUserIdAsync();
            
            if (string.IsNullOrEmpty(userId))
            {
                await ShowErrorAsync("Erro ao obter ID do usuário");
                return;
            }

            // 4. Navegar para a página de videochamada
            var videoCallPage = new Views.VideoCallPage(
                callResponse.AgendamentoId,
                callResponse.CallToken,
                callResponse.HubUrl,
                token ?? "",
                userId,
                "profissional"
            );

            await Shell.Current.Navigation.PushModalAsync(videoCallPage);
        }
        catch (Exception ex)
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine($"Erro ao iniciar videochamada: {ex.Message}");
            await ShowErrorAsync($"Erro ao iniciar videochamada: {ex.Message}");
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

