
using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        Shell.SetNavBarIsVisible(this, false);
    }
}
