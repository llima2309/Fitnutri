using AppFitNutri.Core.Services;
using AppFitNutri.Services;
using AppFitNutri.ViewModel;

namespace AppFitNutri.Views;

public partial class AgendamentosProfissionalPage : ContentPage
{
    public AgendamentosProfissionalPage(
        IAgendamentoService agendamentoService,
        IVideoCallService videoCallService,
        ITokenStore tokenStore)
    {
        InitializeComponent();
        BindingContext = new AgendamentosProfissionalViewModel(agendamentoService, videoCallService, tokenStore);
    }
}

