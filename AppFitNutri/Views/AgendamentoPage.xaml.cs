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
        if (BindingContext is IQueryAttributable attributable)
        {
            attributable.ApplyQueryAttributes(query);
        }
    }
}

