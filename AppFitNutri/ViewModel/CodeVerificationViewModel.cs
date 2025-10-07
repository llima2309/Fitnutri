using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppFitNutri.Core.Services.Login;
using System.Text.RegularExpressions;
using System.Net.Http.Json;

namespace AppFitNutri.ViewModel;

public partial class CodeVerificationViewModel : ObservableObject
{
    private readonly IApiHttp? _apiHttp;
    
    [ObservableProperty] private string? verificationCode;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? errorMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool IsNotBusy => !IsBusy;
    public string VerifyButtonText => IsBusy ? "Verificando..." : "Verificar";

    // Propriedades para callback de resultado
    public string? UserEmail { get; set; }
    public Guid? UserId { get; set; }
    public Action<bool, string?>? OnVerificationComplete { get; set; }

    // Construtor padr�o para uso com service
    public CodeVerificationViewModel()
    {
        InitializePropertyNotifications();
    }

    // Construtor com DI para uso com API
    public CodeVerificationViewModel(IApiHttp apiHttp) : this()
    {
        _apiHttp = apiHttp;
    }

    private void InitializePropertyNotifications()
    {
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(IsBusy))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                OnPropertyChanged(nameof(VerifyButtonText));
            }

            if (e.PropertyName is nameof(ErrorMessage))
            {
                OnPropertyChanged(nameof(HasError));
            }
        };
    }

    [RelayCommand]
    private async Task VerifyCode()
    {
        if (IsBusy) return;

        ErrorMessage = null;
        var code = (VerificationCode ?? "").Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            ErrorMessage = "Digite o c�digo de verifica��o.";
            return;
        }

        if (code.Length < 4)
        {
            ErrorMessage = "C�digo deve ter pelo menos 4 d�gitos.";
            return;
        }

        if (!Regex.IsMatch(code, @"^\d+$"))
        {
            ErrorMessage = "C�digo deve conter apenas n�meros.";
            return;
        }

        if (!int.TryParse(code, out var codeNumber))
        {
            ErrorMessage = "C�digo inv�lido.";
            return;
        }

        if (!UserId.HasValue)
        {
            ErrorMessage = "Erro interno: ID do usu�rio n�o definido.";
            return;
        }

        try
        {
            IsBusy = true;

            if (_apiHttp != null)
            {
                // Usar API real
                var result = await _apiHttp.ConfirmEmailAsync(UserId.Value, codeNumber, CancellationToken.None);
                
                if (result.IsSuccessStatusCode)
                {
                    OnVerificationComplete?.Invoke(true, "E-mail verificado com sucesso!");
                    await ClosePopup();
                }
                else
                {
                    var problem = await result.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    if (problem is not null && problem.TryGetValue("error", out var message) &&
                        !string.IsNullOrWhiteSpace(message))
                    {
                        ErrorMessage = message;
                    }
                    else
                    {
                        ErrorMessage = "C�digo inv�lido. Tente novamente.";
                    }
                }
            }
            else
            {
                // Fallback para simula��o (caso n�o tenha API dispon�vel)
                await SimulateApiCall();
                bool isValid = code.EndsWith("123") || code == "123456";

                if (isValid)
                {
                    OnVerificationComplete?.Invoke(true, "C�digo verificado com sucesso!");
                    await ClosePopup();
                }
                else
                {
                    ErrorMessage = "C�digo inv�lido. Tente novamente.";
                }
            }
        }
        catch (TaskCanceledException)
        {
            ErrorMessage = "Tempo de resposta excedido. Tente novamente.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro inesperado: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResendCode()
    {
        if (IsBusy) return;

        ErrorMessage = null;

        try
        {
            IsBusy = true;

            // Simular envio de novo c�digo - na implementa��o real, 
            // voc� adicionaria um endpoint para reenviar c�digo
            await SimulateApiCall();

            await Application.Current.MainPage.DisplayAlert(
                "C�digo Reenviado", 
                $"Um novo c�digo foi enviado para {UserEmail ?? "seu e-mail"}.", 
                "OK");
        }
        catch (Exception)
        {
            ErrorMessage = "Erro ao reenviar c�digo. Tente novamente.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        OnVerificationComplete?.Invoke(false, "Verifica��o cancelada pelo usu�rio.");
        await ClosePopup();
    }

    private async Task ClosePopup()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

    private static async Task SimulateApiCall()
    {
        // Simula uma chamada de API com delay
        await Task.Delay(1500);
    }

    public void SetUserInfo(string email, Guid userId)
    {
        UserEmail = email;
        UserId = userId;
    }

    public void SetUserEmail(string email)
    {
        UserEmail = email;
    }
}