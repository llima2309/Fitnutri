using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class WorkoutChoicePage : ContentPage
{
    public WorkoutChoicePage(WorkoutChoiceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
