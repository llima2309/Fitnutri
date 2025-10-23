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
    private void EntryCPF_Unfocused(object sender, FocusEventArgs e)
    {
        var vm = BindingContext as UserProfileRegistrationViewModel;
        vm?.ValidateCpfCommand.Execute(null);
    }
    private void EntryRG_Unfocused(object sender, FocusEventArgs e)
    {
        var vm = BindingContext as UserProfileRegistrationViewModel;
        vm?.ValidateRgCommand.Execute(null);
    }
    private void EntryTEL_Unfocused(object sender, FocusEventArgs e)
    {
        var vm = BindingContext as UserProfileRegistrationViewModel;
        vm?.ValidateTelefoneCommand.Execute(null);
    }
    private void EntryCEP_Unfocused(object sender, FocusEventArgs e)
    {
        var vm = BindingContext as UserProfileRegistrationViewModel;
        vm?.ValidateCepCommand.Execute(null);
        if (vm.CEP.Length == 9) // Format: 00000-000
        {
            vm.SearchCepCommand.Execute(vm.CEP);
        }
    }
}
