using AppFitNutri.Views;

namespace AppFitNutri.Services;

public static class LoadingService
{
    private static LoadingPage? _currentLoadingPage;

    public static async Task ShowLoadingAsync()
    {
        if (_currentLoadingPage != null)
            return;

        _currentLoadingPage = new LoadingPage();
        
        // Navegar para a página de loading como modal
        await Shell.Current.Navigation.PushModalAsync(_currentLoadingPage);
    }

    public static async Task HideLoadingAsync()
    {
        if (_currentLoadingPage == null)
            return;

        try
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
        catch (Exception)
        {
            // Ignora erros se a página já foi fechada
        }
        finally
        {
            _currentLoadingPage = null;
        }
    }
}
