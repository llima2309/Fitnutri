using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class AgendamentosPage : ContentPage
{
    public AgendamentosPage(AgendamentosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
