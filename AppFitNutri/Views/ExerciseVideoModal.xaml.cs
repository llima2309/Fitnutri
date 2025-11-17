namespace AppFitNutri.Views;

public partial class ExerciseVideoModal
{
    public ExerciseVideoModal(ViewModel.ExerciseVideoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

