using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class UserProfileRegistrationPage : ContentPage
{
    public UserProfileRegistrationPage(UserProfileRegistrationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        Shell.SetNavBarIsVisible(this, false);
    }
}
