using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class MeusAgendamentosPage : ContentPage
{
    private readonly MeusAgendamentosViewModel _viewModel;
    private bool _isInitialized = false;

    public MeusAgendamentosPage(MeusAgendamentosViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_isInitialized)
        {
            await _viewModel.InicializarAsync();
            _isInitialized = true;
        }
    }
}
