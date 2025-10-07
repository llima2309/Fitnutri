using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppFitNutri.Core.Services;
using AppFitNutri.Core.Models;
using AppFitNutri.Core.Services.Login;
using System.Net.Http.Json;
using Application = Microsoft.Maui.Controls.Application;

namespace AppFitNutri.ViewModel;

public partial class LoginViewModel : ObservableObject
{
    private readonly IApiHttp _authApi;
    private readonly ITokenStore _tokenStore;
    private readonly IProfileService _profileService;

    [ObservableProperty] private string? emailOrUserName;
    [ObservableProperty] private string? password;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private bool mostrarSenha;

    public bool IsPassword => !MostrarSenha;
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool IsNotBusy => !IsBusy;
    public string LoginButtonText => IsBusy ? "Entrando..." : "Entrar";

    public LoginViewModel(IApiHttp authApi, ITokenStore tokenStore, IProfileService profileService)
    {
        _authApi = authApi;
        _tokenStore = tokenStore;
        _profileService = profileService;
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

    partial void OnMostrarSenhaChanged(bool value)
    {
        OnPropertyChanged(nameof(IsPassword));
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
            var result = await _authApi.LoginAsync(req, CancellationToken.None);

            if (result.IsSuccessStatusCode)
            {
                await ProcessSuccessfulLogin(result);
            }
            else
            {
                await ProcessLoginError(result, user);
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

    private async Task ProcessSuccessfulLogin(HttpResponseMessage result)
    {
        var content = await result.Content.ReadFromJsonAsync<AuthResponse>();
        if (content != null)
        {
            await _tokenStore.SaveAsync(content.AccessToken, content.ExpiresAt);
            var validationResult = await _authApi.ValidaToken();
            
            if (validationResult.IsSuccessStatusCode)
            {
                var meResponse = await validationResult.Content.ReadFromJsonAsync<MeResponse>();
                await Application.Current.MainPage.DisplayAlert("Sucesso", "Login realizado com sucesso!", "OK");
                
                // Verificar se o usuário tem perfil associado
                if (await CheckUserHasProfile())
                {
                    // Usuário tem perfil, navegar para a página principal
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    // Usuário não tem perfil, navegar para seleção de perfil
                    await Shell.Current.GoToAsync($"//{nameof(Views.ProfileSelectionPage)}");
                }
            }
        }
    }

    private async Task<bool> CheckUserHasProfile()
    {
        try
        {
            // Verificar se o usuário tem perfis associados através da API
            var perfis = await _profileService.ObterMeusPerfisAsync();
            return perfis?.Any() == true;
        }
        catch (Exception ex)
        {
            // Em caso de erro, assumir que precisa selecionar perfil
            System.Diagnostics.Debug.WriteLine($"Erro ao verificar perfil: {ex.Message}");
            return false;
        }
    }

    private async Task ProcessLoginError(HttpResponseMessage result, string userInput)
    {
        var problem = await result.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        
        if (problem is not null && problem.TryGetValue("error", out var message) && 
            !string.IsNullOrWhiteSpace(message))
        {
            // Verifica se o erro é relacionado ao e-mail não verificado
            if (message.Contains("E-mail não verificado"))
            {
                await HandleEmailNotVerified(userInput);
            }
            else
            {
                ErrorMessage = message;
            }
        }
        else
        {
            ErrorMessage = "Erro desconhecido ao fazer login.";
        }
    }

    private async Task HandleEmailNotVerified(string userInput)
    {
        try
        {
            var choice = await Application.Current.MainPage.DisplayAlert(
                "E-mail Não Verificado",
                "Seu e-mail ainda não foi verificado. Você recebeu um código por e-mail quando sua conta foi aprovada. Deseja inserir o código agora?",
                "Sim, Verificar",
                "Cancelar");

            if (choice)
            {
                await ShowEmailVerificationPopup(userInput);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao processar verificação: {ex.Message}";
        }
    }

    private async Task ShowEmailVerificationPopup(string userInput)
    {
        try
        {
            // Criar um ViewModel customizado para este cenário
            var viewModel = new EmailVerificationViewModel(_authApi, userInput);
            
            viewModel.OnVerificationComplete = async (success, message) =>
            {
                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Sucesso", 
                        "E-mail verificado com sucesso! Tente fazer login novamente.", 
                        "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Erro", 
                        message ?? "Verificação não concluída.", 
                        "OK");
                }
            };

            var popup = new Views.CodeVerificationPopup(viewModel);
            await Shell.Current.Navigation.PushModalAsync(popup);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro na verificação: {ex.Message}";
        }
    }

    [RelayCommand]
    private Task IrParaRegistrarAsync() => Shell.Current.GoToAsync(nameof(AppFitNutri.Views.RegisterPage));

    [RelayCommand]
    private Task IrParaEsqueciSenhaAsync() => Shell.Current.GoToAsync(nameof(Views.ForgotPasswordPage));
}