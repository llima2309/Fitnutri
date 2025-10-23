using System.Collections.Generic;
using AppFitNutri.ViewModel;
using Microsoft.Maui.Controls;

namespace AppFitNutri.Views;

public partial class AgendamentoPage : ContentPage, IQueryAttributable
{
    public AgendamentoPage(AgendamentoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is AgendamentoViewModel viewModel)
        {
            if (query.TryGetValue("Profissional", out var profObj) && profObj is Models.Profissional p)
            {
                viewModel.SetProfissional(p);
            }
            else if (query.TryGetValue("profissional", out var profObj2) && profObj2 is Models.Profissional p2)
            {
                viewModel.SetProfissional(p2);
            }
        }
    }
}

