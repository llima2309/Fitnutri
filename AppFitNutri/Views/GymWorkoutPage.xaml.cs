using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class GymWorkoutPage
{
    public GymWorkoutPage()
    {
        InitializeComponent();
        BindingContext = new GymWorkoutViewModel();
    }
}

