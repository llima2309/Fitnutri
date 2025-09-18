using AppFitNutri.Views;
using AppFitNutri.ViewModel;
using AppFitNutri.Core.Services.Login;

namespace AppFitNutri.Services;

public static class CodeVerificationService
{
    /// <summary>
    /// Exibe o popup de verifica��o de c�digo com informa��es do usu�rio
    /// </summary>
    /// <param name="userEmail">E-mail do usu�rio</param>
    /// <param name="userId">ID do usu�rio</param>
    /// <param name="apiHttp">Servi�o de API (opcional)</param>
    /// <param name="onComplete">Callback chamado quando a verifica��o for conclu�da</param>
    /// <returns>Task</returns>
    public static async Task ShowCodeVerificationPopupAsync(
        string userEmail,
        Guid userId,
        IApiHttp? apiHttp,
        Action<bool, string?> onComplete)
    {
        try
        {
            // Cria uma nova inst�ncia do ViewModel
            var viewModel = apiHttp != null ? new CodeVerificationViewModel(apiHttp) : new CodeVerificationViewModel();
            viewModel.SetUserInfo(userEmail, userId);
            viewModel.OnVerificationComplete = onComplete;

            // Cria a p�gina popup
            var popup = new CodeVerificationPopup(viewModel);

            // Exibe como modal
            await Shell.Current.Navigation.PushModalAsync(popup);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erro", 
                $"Erro ao exibir verifica��o de c�digo: {ex.Message}", 
                "OK");
        }
    }

    /// <summary>
    /// Exibe o popup de verifica��o de c�digo com awaitable result
    /// </summary>
    /// <param name="userEmail">E-mail do usu�rio</param>
    /// <param name="userId">ID do usu�rio</param>
    /// <param name="apiHttp">Servi�o de API (opcional)</param>
    /// <returns>Task<(bool Success, string Message)></returns>
    public static async Task<(bool Success, string Message)> ShowCodeVerificationPopupAsync(
        string userEmail, 
        Guid userId, 
        IApiHttp? apiHttp = null)
    {
        var tcs = new TaskCompletionSource<(bool, string)>();

        await ShowCodeVerificationPopupAsync(userEmail, userId, apiHttp, (success, message) =>
        {
            tcs.SetResult((success, message ?? string.Empty));
        });

        return await tcs.Task;
    }

    /// <summary>
    /// M�todo legacy para compatibilidade (apenas simula��o)
    /// </summary>
    [Obsolete("Use o m�todo que aceita userId e apiHttp para funcionalidade completa")]
    public static async Task ShowCodeVerificationPopupAsync(
        string userEmail, 
        Action<bool, string?> onComplete)
    {
        await ShowCodeVerificationPopupAsync(userEmail, Guid.Empty, null, onComplete);
    }

    /// <summary>
    /// M�todo legacy para compatibilidade (apenas simula��o)
    /// </summary>
    [Obsolete("Use o m�todo que aceita userId e apiHttp para funcionalidade completa")]
    public static async Task<(bool Success, string Message)> ShowCodeVerificationPopupAsync(string userEmail)
    {
        return await ShowCodeVerificationPopupAsync(userEmail, Guid.Empty, null);
    }
}