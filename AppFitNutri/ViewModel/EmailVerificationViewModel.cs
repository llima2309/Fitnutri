using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppFitNutri.Core.Services.Login;
using System.Net.Http.Json;

namespace AppFitNutri.ViewModel;

/// <summary>
/// ViewModel especializado para verifica��o de e-mail durante o login
/// </summary>
public partial class EmailVerificationViewModel : ObservableObject
{
    private readonly IApiHttp _apiHttp;
    private readonly string _userIdentifier;

    [ObservableProperty] private string? verificationCode;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? errorMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool IsNotBusy => !IsBusy;
    public string VerifyButtonText => IsBusy ? "Verificando..." : "Verificar";

    public string? UserEmail => _userIdentifier;
    public Action<bool, string?>? OnVerificationComplete { get; set; }

    public EmailVerificationViewModel(IApiHttp apiHttp, string userIdentifier)
    {
        _apiHttp = apiHttp;
        _userIdentifier = userIdentifier;
        
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
    public async Task VerifyCode()
    {
        if (IsBusy) return;

        ErrorMessage = null;
        var code = (VerificationCode ?? "").Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            ErrorMessage = "Digite o c�digo de verifica��o.";
            return;
        }

        if (!int.TryParse(code, out var codeNumber))
        {
            ErrorMessage = "C�digo deve conter apenas n�meros.";
            return;
        }

        try
        {
            IsBusy = true;

            // Usar o novo m�todo que aceita email/username
            var result = await _apiHttp.ConfirmEmailByIdentifierAsync(_userIdentifier, codeNumber, CancellationToken.None);

            if (result.IsSuccessStatusCode)
            {
                // Primeiro fecha o modal para evitar conflitos na pilha de navegação
                await Shell.Current.Navigation.PopModalAsync();
                
                // Depois invoca o callback
                OnVerificationComplete?.Invoke(true, "E-mail verificado com sucesso!");
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
    public async Task ResendCode()
    {
        await Application.Current.MainPage.DisplayAlert(
            "Reenvio", 
            "Para reenviar o c�digo, entre em contato com o administrador.", 
            "OK");
    }

    [RelayCommand]
    public async Task Cancel()
    {
        OnVerificationComplete?.Invoke(false, "Verifica��o cancelada pelo usu�rio.");
        await Shell.Current.Navigation.PopModalAsync();
    }
}