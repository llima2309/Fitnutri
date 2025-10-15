using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class MeusAgendamentosPage : ContentPage
{
    public MeusAgendamentosPage(MeusAgendamentosViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
