using AppFitNutri.Views;

namespace AppFitNutri;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; }

    public App(IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
        InitializeComponent();
    }
    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Cria uma nova janela com a LoginPage como página principal
        var loginPage = Services.GetService(typeof(LoginPage)) as Page;
        return new Window(loginPage);
    }
}
