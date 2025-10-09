using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class ListaProfissionaisPage : ContentPage
{
    public ListaProfissionaisPage(ListaProfissionaisViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
