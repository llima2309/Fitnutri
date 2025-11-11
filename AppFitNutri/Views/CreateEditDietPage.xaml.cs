using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class CreateEditDietPage
{
    public CreateEditDietPage(CreateEditDietViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
