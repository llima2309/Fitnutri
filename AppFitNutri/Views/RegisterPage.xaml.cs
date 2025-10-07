using AppFitNutri.ViewModels;

namespace AppFitNutri.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        Shell.SetNavBarIsVisible(this, false);
    }
}
