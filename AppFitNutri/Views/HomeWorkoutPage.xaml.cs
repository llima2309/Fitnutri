using AppFitNutri.Models;
using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class HomeWorkoutPage : ContentPage
{
    private HomeWorkoutViewModel ViewModel => (HomeWorkoutViewModel)BindingContext;

    public HomeWorkoutPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
