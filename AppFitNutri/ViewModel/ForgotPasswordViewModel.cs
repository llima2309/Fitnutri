using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppFitNutri.Core.Services.Login;
using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;
using Application = Microsoft.Maui.Controls.Application;

namespace AppFitNutri.ViewModel;

public partial class ForgotPasswordViewModel : ObservableObject
{
    private readonly IApiHttp _authApi;

    [ObservableProperty] private string? email;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string? successMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);
    public bool IsNotBusy => !IsBusy;
    public string SendButtonText => IsBusy ? "Enviando..." : "Enviar instruções";

    public ForgotPasswordViewModel(IApiHttp authApi)
    {
        _authApi = authApi;
        
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(IsBusy))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                OnPropertyChanged(nameof(SendButtonText));
            }

            if (e.PropertyName is nameof(ErrorMessage))
            {
                OnPropertyChanged(nameof(HasError));
            }

            if (e.PropertyName is nameof(SuccessMessage))
            {
                OnPropertyChanged(nameof(HasSuccess));
            }
        };
    }

    [RelayCommand]
    private async Task EnviarSolicitacao()
    {
        if (IsBusy) return;

        ErrorMessage = null;
        SuccessMessage = null;

        var emailTrimmed = (Email ?? "").Trim();

        if (string.IsNullOrWhiteSpace(emailTrimmed))
        {
            ErrorMessage = "Informe um e-mail válido.";
            return;
        }

        if (!IsValidEmail(emailTrimmed))
        {
            ErrorMessage = "Digite um e-mail válido.";
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _authApi.ForgotPasswordAsync(emailTrimmed, CancellationToken.None);

            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
                if (content != null)
                {
                    SuccessMessage = content.Message;
                    Email = ""; // Limpa o campo
                }
                else
                {
                    SuccessMessage = "Se o e-mail existir em nossa base, você receberá instruções para redefinir sua senha.";
                }
            }
            else
            {
                var problem = await result.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (problem is not null && problem.TryGetValue("error", out var message) && 
                    !string.IsNullOrWhiteSpace(message))
                    ErrorMessage = message;
                else
                    ErrorMessage = "Erro ao processar solicitação. Tente novamente.";
            }
        }
        catch (TaskCanceledException)
        {
            ErrorMessage = "Tempo de resposta excedido. Tente novamente.";
        }
        catch (Exception)
        {
            ErrorMessage = "Erro inesperado. Verifique sua conexão.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task VoltarParaLogin()
    {
        await Shell.Current.GoToAsync("..");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public class ForgotPasswordResponse
    {
        public string Message { get; set; } = "";
    }
}