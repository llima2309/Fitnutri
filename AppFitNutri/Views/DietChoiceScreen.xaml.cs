using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class DietChoiceScreen : ContentPage
{
    public DietChoiceScreen(DietChoiceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
