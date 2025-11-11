using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class DietDetailPage : ContentPage
{
    public DietDetailPage(DietDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

