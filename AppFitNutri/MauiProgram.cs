using AppFitNutri.Core.Services;
using AppFitNutri.Core.Services.Login;
using AppFitNutri.Services;
using AppFitNutri.ViewModel;

namespace AppFitNutri;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // HttpClient nomeado para a API de Auth
        builder.Services.AddHttpClient<IApiHttp, ApiHttp>(client =>
        {
            client.BaseAddress = new Uri("https://api.fit-nutri.com");
            //client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("x-api-key", "<STRONG_CLIENT_KEY>");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // HttpClient para ProfileService
        builder.Services.AddHttpClient<IProfileService, ProfileService>(client =>
        {
            client.BaseAddress = new Uri("https://api.fit-nutri.com");
            client.DefaultRequestHeaders.Add("x-api-key", "<STRONG_CLIENT_KEY>");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Token store
        builder.Services.AddSingleton<ITokenStore, SecureTokenStore>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<AppFitNutri.ViewModels.RegisterViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<ResetPasswordViewModel>();
        builder.Services.AddTransient<EmailVerificationViewModel>();

        // Views
        builder.Services.AddTransient<Views.LoginPage>();
        builder.Services.AddTransient<AppFitNutri.Views.RegisterPage>();
        builder.Services.AddTransient<Views.ForgotPasswordPage>();
        builder.Services.AddTransient<Views.ResetPasswordPage>();
        builder.Services.AddTransient<Views.CodeVerificationPopup>();
        builder.Services.AddTransient<Views.ProfileSelectionPage>();

        return builder.Build();
    }
}
