using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class HomeNutricionistaPage : ContentPage
{
    public HomeNutricionistaPage()
    {
        InitializeComponent();
        BindingContext = new HomeNutricionistaViewModel();
    }
}

