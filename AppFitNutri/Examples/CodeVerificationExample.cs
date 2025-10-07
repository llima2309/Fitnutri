using AppFitNutri.Services;
using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

/// <summary>
/// Exemplo de como usar o popup de verificação de código
/// </summary>
public static class CodeVerificationExample
{
    /// <summary>
    /// Exemplo básico de uso do popup
    /// </summary>
    public static async Task ShowExampleAsync()
    {
        try
        {
            // Exemplo 1: Usando o método com callback
            await CodeVerificationService.ShowCodeVerificationPopupAsync(
                "usuario@exemplo.com",
                (success, message) =>
                {
                    if (success)
                    {
                        Application.Current.MainPage.DisplayAlert("Sucesso", message, "OK");
                    }
                    else
                    {
                        Application.Current.MainPage.DisplayAlert("Falha", message, "OK");
                    }
                });

            // Exemplo 2: Usando o método awaitable
            var result = await CodeVerificationService.ShowCodeVerificationPopupAsync("usuario@exemplo.com");
            
            if (result.Success)
            {
                await Application.Current.MainPage.DisplayAlert("Verificado", result.Message, "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Não Verificado", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    /// <summary>
    /// Exemplo de integração com fluxo de registro
    /// </summary>
    public static async Task<bool> VerifyEmailDuringRegistrationAsync(string email)
    {
        try
        {
            var result = await CodeVerificationService.ShowCodeVerificationPopupAsync(email);
            return result.Success;
        }
        catch
        {
            return false;
        }
    }
}