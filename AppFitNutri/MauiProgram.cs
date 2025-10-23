using AppFitNutri.Core.Services;
using AppFitNutri.Core.Services.Login;
using AppFitNutri.Services;
using AppFitNutri.ViewModel;
using Microsoft.Maui.Handlers;
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

        // HttpClient para UserProfileService
        builder.Services.AddHttpClient<IUserProfileService, UserProfileService>(client =>
        {
            client.BaseAddress = new Uri("https://api.fit-nutri.com");
            client.DefaultRequestHeaders.Add("x-api-key", "<STRONG_CLIENT_KEY>");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // HttpClient para ProfissionaisService
        builder.Services.AddHttpClient<IProfissionaisService, ProfissionaisService>(client =>
        {
            client.BaseAddress = new Uri("https://api.fit-nutri.com");
            client.DefaultRequestHeaders.Add("x-api-key", "<STRONG_CLIENT_KEY>");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // HttpClient para AgendamentoService
        builder.Services.AddHttpClient<IAgendamentoService, AgendamentoService>(client =>
        {
            client.BaseAddress = new Uri("https://api.fit-nutri.com");
            client.DefaultRequestHeaders.Add("x-api-key", "<STRONG_CLIENT_KEY>");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Token store
        builder.Services.AddSingleton<ITokenStore, SecureTokenStore>();
        builder.Services.AddSingleton<SecureTokenStore>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<AppFitNutri.ViewModels.RegisterViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<ResetPasswordViewModel>();
        builder.Services.AddTransient<EmailVerificationViewModel>();
        builder.Services.AddTransient<ProfileSelectionViewModel>();
        builder.Services.AddTransient<UserProfileRegistrationViewModel>();
        builder.Services.AddTransient<HomePageViewModel>();
        builder.Services.AddTransient<AgendamentosViewModel>();
        builder.Services.AddTransient<ListaProfissionaisViewModel>();
        builder.Services.AddTransient<AgendamentoViewModel>();
        builder.Services.AddTransient<MeusAgendamentosViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();

        // Views
        builder.Services.AddTransient<Views.LoginPage>();
        builder.Services.AddTransient<AppFitNutri.Views.RegisterPage>();
        builder.Services.AddTransient<Views.ForgotPasswordPage>();
        builder.Services.AddTransient<Views.ResetPasswordPage>();
        builder.Services.AddTransient<Views.CodeVerificationPopup>();
        builder.Services.AddTransient<Views.LoadingPage>();
        builder.Services.AddTransient<Views.ProfileSelectionPage>();
        builder.Services.AddTransient<Views.UserProfileRegistrationPage>();
        builder.Services.AddTransient<Views.HomePage>();
        builder.Services.AddTransient<Views.AgendamentosPage>();
        builder.Services.AddTransient<Views.AgendamentoPage>();
        builder.Services.AddTransient<Views.ListaProfissionaisPage>();
        builder.Services.AddTransient<Views.PerfilPage>();
        builder.Services.AddTransient<Views.MeusAgendamentosPage>();
        return builder.Build();
    }
}
