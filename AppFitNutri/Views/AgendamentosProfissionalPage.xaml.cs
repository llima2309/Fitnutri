using AppFitNutri.Services;
using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class AgendamentosProfissionalPage : ContentPage
{
    public AgendamentosProfissionalPage(IAgendamentoService agendamentoService)
    {
        InitializeComponent();
        BindingContext = new AgendamentosProfissionalViewModel(agendamentoService);
    }
}

