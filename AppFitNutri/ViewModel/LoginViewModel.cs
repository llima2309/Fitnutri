using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppFitNutri.Core.Services;
using static System.Net.Mime.MediaTypeNames;
using AppFitNutri.Core.Models;
using Application = Microsoft.Maui.Controls.Application;
using AppFitNutri.Core.Services.Login;
using System.Net.Http.Json;

namespace AppFitNutri.ViewModel;

public partial class LoginViewModel : ObservableObject
{
    private readonly IApiHttp _authApi;
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

    public LoginViewModel(IApiHttp authApi, ITokenStore tokenStore)
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

            var req = new LoginRequest(user, pass);
            var result = await _authApi.PostAsyncLogin(req, CancellationToken.None);

            if (result.IsSuccessStatusCode)
            {
                AuthResponse? content = await result.Content.ReadFromJsonAsync<AuthResponse>();
                if (content != null)
                {
                    await _tokenStore.SaveAsync(content.AccessToken, content.ExpiresAt);
                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Login realizado com sucesso.", "OK");
                }
            }
            else
            {
                var problem = await result.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (problem is not null && problem.TryGetValue("error", out var message) && !string.IsNullOrWhiteSpace(message))
                    ErrorMessage = message;
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
