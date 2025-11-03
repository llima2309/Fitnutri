using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class HomeWorkoutPage
{
    public HomeWorkoutPage()
    {
        InitializeComponent();
        BindingContext = new HomeWorkoutViewModel();
    }
}

