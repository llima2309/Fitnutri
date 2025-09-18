using AppFitNutri.Views;
using AppFitNutri.ViewModel;
using AppFitNutri.Core.Services.Login;

namespace AppFitNutri.Services;

public static class CodeVerificationService
{
    /// <summary>
    /// Exibe o popup de verificação de código com informações do usuário
    /// </summary>
    /// <param name="userEmail">E-mail do usuário</param>
    /// <param name="userId">ID do usuário</param>
    /// <param name="apiHttp">Serviço de API (opcional)</param>
    /// <param name="onComplete">Callback chamado quando a verificação for concluída</param>
    /// <returns>Task</returns>
    public static async Task ShowCodeVerificationPopupAsync(
        string userEmail,
        Guid userId,
        IApiHttp? apiHttp,
        Action<bool, string?> onComplete)
    {
        try
        {
            // Cria uma nova instância do ViewModel
            var viewModel = apiHttp != null ? new CodeVerificationViewModel(apiHttp) : new CodeVerificationViewModel();
            viewModel.SetUserInfo(userEmail, userId);
            viewModel.OnVerificationComplete = onComplete;

            // Cria a página popup
            var popup = new CodeVerificationPopup(viewModel);

            // Exibe como modal
            await Shell.Current.Navigation.PushModalAsync(popup);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erro", 
                $"Erro ao exibir verificação de código: {ex.Message}", 
                "OK");
        }
    }

    /// <summary>
    /// Exibe o popup de verificação de código com awaitable result
    /// </summary>
    /// <param name="userEmail">E-mail do usuário</param>
    /// <param name="userId">ID do usuário</param>
    /// <param name="apiHttp">Serviço de API (opcional)</param>
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
    /// Método legacy para compatibilidade (apenas simulação)
    /// </summary>
    [Obsolete("Use o método que aceita userId e apiHttp para funcionalidade completa")]
    public static async Task ShowCodeVerificationPopupAsync(
        string userEmail, 
        Action<bool, string?> onComplete)
    {
        await ShowCodeVerificationPopupAsync(userEmail, Guid.Empty, null, onComplete);
    }

    /// <summary>
    /// Método legacy para compatibilidade (apenas simulação)
    /// </summary>
    [Obsolete("Use o método que aceita userId e apiHttp para funcionalidade completa")]
    public static async Task<(bool Success, string Message)> ShowCodeVerificationPopupAsync(string userEmail)
    {
        return await ShowCodeVerificationPopupAsync(userEmail, Guid.Empty, null);
    }
}