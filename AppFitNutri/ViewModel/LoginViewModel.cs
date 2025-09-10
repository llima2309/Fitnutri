using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppFitNutri.Core.Services;
using static System.Net.Mime.MediaTypeNames;
using AppFitNutri.Core.Models;
using Application = Microsoft.Maui.Controls.Application;

namespace AppFitNutri.ViewModel;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthApi _authApi;
    private readonly ITokenStore _tokenStore;

    [ObservableProperty] private string? emailOrUserName;
    [ObservableProperty] private string? password;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? errorMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool IsNotBusy => !IsBusy;
    public string LoginButtonText => IsBusy ? "Entrando..." : "Entrar";

    // Delegate for alert display, to be set by the View or platform code
    public Func<string, string, string, Task>? ShowAlert { get; set; }

    public LoginViewModel(IAuthApi authApi, ITokenStore tokenStore)
    {
        _authApi = authApi;
        _tokenStore = tokenStore;
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(IsBusy))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                OnPropertyChanged(nameof(LoginButtonText));
            }
            if (e.PropertyName is nameof(ErrorMessage))
            {
                OnPropertyChanged(nameof(HasError));
            }
        };
    }

    [RelayCommand]
    private async Task Entrar()
    {
        if (IsBusy) return;

        ErrorMessage = null;

        var user = (EmailOrUserName ?? "").Trim();
        var pass = Password ?? "";

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            ErrorMessage = "Informe login/e-mail e senha.";
            return;
        }

        try
        {
            IsBusy = true;

            var req = new LoginRequest { UserNameOrEmail = user, Password = pass };
            var result = await _authApi.LoginAsync(req, CancellationToken.None);

            if (result.IsSuccess)
            {
                await _tokenStore.SaveAsync(result.Token!, result.Exp);
                await Application.Current.MainPage.DisplayAlert("Sucesso", "Login realizado com sucesso.", "OK");
            }
            else
            {
                ErrorMessage = result.Error ?? "Falha ao autenticar.";
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
}
