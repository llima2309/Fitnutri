using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class CodeVerificationPopup : ContentPage
{
    public CodeVerificationPopup(CodeVerificationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        // Auto-focus no campo de c�digo quando a p�gina carregar
        Loaded += OnPageLoaded;
    }

    // Construtor adicional para EmailVerificationViewModel
    public CodeVerificationPopup(EmailVerificationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        // Auto-focus no campo de c�digo quando a p�gina carregar
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, EventArgs e)
    {
        // Pequeno delay para garantir que a p�gina esteja totalmente carregada
        await Task.Delay(300);
        CodeEntry.Focus();
    }
}