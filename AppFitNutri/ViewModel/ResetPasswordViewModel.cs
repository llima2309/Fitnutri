using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppFitNutri.Core.Services.Login;
using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Application = Microsoft.Maui.Controls.Application;

namespace AppFitNutri.ViewModel;

public partial class ResetPasswordViewModel : ObservableObject
{
    private readonly IApiHttp _authApi;
    private readonly string _token;

    [ObservableProperty] private string? newPassword;
    [ObservableProperty] private string? confirmPassword;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string? successMessage;
    [ObservableProperty] private bool tokenValido;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);
    public bool IsNotBusy => !IsBusy;
    public string ResetButtonText => IsBusy ? "Redefinindo..." : "Redefinir senha";

    // Propriedades para validação visual da senha
    public bool HasMinLength => !string.IsNullOrEmpty(NewPassword) && NewPassword.Length >= 8;
    public bool HasLowercase => !string.IsNullOrEmpty(NewPassword) && NewPassword.Any(char.IsLower);
    public bool HasUppercase => !string.IsNullOrEmpty(NewPassword) && NewPassword.Any(char.IsUpper);
    public bool HasNumber => !string.IsNullOrEmpty(NewPassword) && NewPassword.Any(char.IsDigit);
    public bool HasSpecialChar => !string.IsNullOrEmpty(NewPassword) && NewPassword.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c));
    public bool PasswordsMatch => !string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(ConfirmPassword) && NewPassword == ConfirmPassword;
    
    public bool IsFormValid => HasMinLength && HasLowercase && HasUppercase && HasNumber && HasSpecialChar && PasswordsMatch;

    // Cores para os indicadores
    public Color MinLengthColor => HasMinLength ? Colors.Green : Colors.Red;
    public Color LowercaseColor => HasLowercase ? Colors.Green : Colors.Red;
    public Color UppercaseColor => HasUppercase ? Colors.Green : Colors.Red;
    public Color NumberColor => HasNumber ? Colors.Green : Colors.Red;
    public Color SpecialCharColor => HasSpecialChar ? Colors.Green : Colors.Red;
    public Color PasswordsMatchColor => PasswordsMatch ? Colors.Green : Colors.Red;

    // Textos para os indicadores
    public string MinLengthText => HasMinLength ? "[OK] Mínimo 8 caracteres" : "[X] Mínimo 8 caracteres";
    public string LowercaseText => HasLowercase ? "[OK] Pelo menos 1 letra minúscula" : "[X] Pelo menos 1 letra minúscula";
    public string UppercaseText => HasUppercase ? "[OK] Pelo menos 1 letra maiúscula" : "[X] Pelo menos 1 letra maiúscula";
    public string NumberText => HasNumber ? "[OK] Pelo menos 1 número" : "[X] Pelo menos 1 número";
    public string SpecialCharText => HasSpecialChar ? "[OK] Pelo menos 1 caractere especial" : "[X] Pelo menos 1 caractere especial";
    public string PasswordsMatchText => PasswordsMatch ? "[OK] Senhas coincidem" : "[X] Senhas coincidem";

    public ResetPasswordViewModel(IApiHttp authApi, string token)
    {
        _authApi = authApi;
        _token = token;
        TokenValido = !string.IsNullOrEmpty(token);
        
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(IsBusy))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                OnPropertyChanged(nameof(ResetButtonText));
            }

            if (e.PropertyName is nameof(ErrorMessage))
            {
                OnPropertyChanged(nameof(HasError));
            }

            if (e.PropertyName is nameof(SuccessMessage))
            {
                OnPropertyChanged(nameof(HasSuccess));
            }

            if (e.PropertyName is nameof(NewPassword))
            {
                UpdatePasswordValidation();
            }

            if (e.PropertyName is nameof(ConfirmPassword))
            {
                OnPropertyChanged(nameof(PasswordsMatch));
                OnPropertyChanged(nameof(PasswordsMatchColor));
                OnPropertyChanged(nameof(PasswordsMatchText));
                OnPropertyChanged(nameof(IsFormValid));
            }
        };
    }

    private void UpdatePasswordValidation()
    {
        OnPropertyChanged(nameof(HasMinLength));
        OnPropertyChanged(nameof(HasLowercase));
        OnPropertyChanged(nameof(HasUppercase));
        OnPropertyChanged(nameof(HasNumber));
        OnPropertyChanged(nameof(HasSpecialChar));
        OnPropertyChanged(nameof(PasswordsMatch));
        OnPropertyChanged(nameof(IsFormValid));

        OnPropertyChanged(nameof(MinLengthColor));
        OnPropertyChanged(nameof(LowercaseColor));
        OnPropertyChanged(nameof(UppercaseColor));
        OnPropertyChanged(nameof(NumberColor));
        OnPropertyChanged(nameof(SpecialCharColor));
        OnPropertyChanged(nameof(PasswordsMatchColor));

        OnPropertyChanged(nameof(MinLengthText));
        OnPropertyChanged(nameof(LowercaseText));
        OnPropertyChanged(nameof(UppercaseText));
        OnPropertyChanged(nameof(NumberText));
        OnPropertyChanged(nameof(SpecialCharText));
        OnPropertyChanged(nameof(PasswordsMatchText));
    }

    [RelayCommand]
    private async Task RedefinirSenha()
    {
        if (IsBusy || string.IsNullOrEmpty(_token)) return;

        ErrorMessage = null;
        SuccessMessage = null;

        if (!IsFormValid)
        {
            ErrorMessage = "Por favor, preencha todos os campos corretamente.";
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _authApi.ResetPasswordAsync(_token, NewPassword!, CancellationToken.None);

            if (result.IsSuccessStatusCode)
            {
                SuccessMessage = "Senha redefinida com sucesso! Você já pode fazer login com sua nova senha.";
                NewPassword = "";
                ConfirmPassword = "";
            }
            else
            {
                var problem = await result.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (problem is not null && problem.TryGetValue("error", out var message) && 
                    !string.IsNullOrWhiteSpace(message))
                    ErrorMessage = message;
                else
                    ErrorMessage = "Erro ao redefinir senha. Tente novamente.";
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
    private async Task IrParaLogin()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    [RelayCommand]
    private async Task IrParaEsqueciSenha()
    {
        await Shell.Current.GoToAsync("//ForgotPasswordPage");
    }
}