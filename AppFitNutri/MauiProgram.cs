using AppFitNutri.Core.Services;
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
        builder.Services.AddHttpClient<IAuthApi, AuthApiHttp>(client =>
        {
            client.BaseAddress = new Uri("http://fitnutri-alb-1998611476.us-east-1.elb.amazonaws.com");
            //client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("x-api-key", "<STRONG_CLIENT_KEY>");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // Token store
        builder.Services.AddSingleton<ITokenStore, SecureTokenStore>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();

        // Views
        builder.Services.AddTransient<Views.LoginPage>();

        return builder.Build();
    }
}
