using System.Net.Http.Json;
using AppFitNutri.Core;
using AppFitNutri.Core.Models;
using AppFitNutri.Core.Services.Login;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Xunit.Abstractions;

namespace AppFitNutri.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IApiHttp _authApi;


    public RegisterViewModel(IApiHttp authApi)
    {
        _authApi = authApi;
    }

    // Propriedades observáveis
    [ObservableProperty] private string userName;
    [ObservableProperty] private string email;
    [ObservableProperty] private string password;
    [ObservableProperty] private string confirmPassword;
    [ObservableProperty] private bool mostrarSenha;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string errorMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool IsNotBusy => !IsBusy;

    // Comandos
    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task RegistrarAsync()
    {
        if (IsBusy) return;
        ErrorMessage = string.Empty;

        // validações básicas
        if (string.IsNullOrWhiteSpace(UserName)) { ErrorMessage = "Informe o usuário."; return; }
        if(!ValidatorsCore.IsValidUserName(userName)) { ErrorMessage = "Username inválido. Use somente letras e números (3–32)."; return; }
        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@') || !Email.Contains('.'))
        { ErrorMessage = "E-mail inválido."; return; }
        if (string.IsNullOrWhiteSpace(Password) || !ValidatorsCore.IsStrongPassword(Password))
        { ErrorMessage = "Senha fraca. Mín. 8, com minúscula, maiúscula, número e caractere especial."; return; }
        if (Password != ConfirmPassword)
        { ErrorMessage = "As senhas não conferem."; return; }

        try
        {
            IsBusy = true;
            RegistrarCommand.NotifyCanExecuteChanged();

            var payload = new { userName = UserName.Trim(), email = Email.Trim(), password = Password };
            RegisterRequest registerRequest = new RegisterRequest(UserName.Trim(), Email.Trim(), Password);
            var resp = await _authApi.RegisterAsync(registerRequest,CancellationToken.None);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ErrorMessage = $"Falha ao registrar - {body}";
                return;
            }
            RegisterResponse content = await resp.Content.ReadFromJsonAsync<RegisterResponse>();
            await Shell.Current.DisplayAlert("Sucesso", content.message, "OK");
            await Shell.Current.GoToAsync(".."); // volta ao Login
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro inesperado: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RegistrarCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private Task VoltarLoginAsync() => Shell.Current.GoToAsync("..");
}
