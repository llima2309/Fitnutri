using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class DietListPage : ContentPage
{
    private readonly DietListViewModel _viewModel;

    public DietListPage(DietListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDietsCommand.ExecuteAsync(null);
    }
}
