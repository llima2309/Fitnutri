using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Models;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using AppFitNutri.Services;

namespace AppFitNutri.ViewModel;

public class AgendamentoViewModel : INotifyPropertyChanged, IQueryAttributable
{
    private Profissional? _profissional;
    private DateTime _selectedDate;
    private string _selectedTime = string.Empty;

    private readonly IAgendamentoService _agendamentoService;

    public ObservableCollection<DateTime> AvailableDates { get; } = new();
    public ObservableCollection<string> AvailableTimes { get; } = new();

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }

    public AgendamentoViewModel(IAgendamentoService agendamentoService)
    {
        _agendamentoService = agendamentoService;
        var today = DateTime.Today;
        for (int i = 0; i < 14; i++)
        {
            AvailableDates.Add(today.AddDays(i));
        }

        SelectedDate = AvailableDates.Count > 0 ? AvailableDates[0] : today;

        ConfirmCommand = new Command(OnConfirm);
        CancelCommand = new Command(OnCancel);

        // Carrega os horários iniciais
        _ = LoadDisponibilidadeAsync();
    }

    public Profissional? Profissional
    {
        get => _profissional;
        set
        {
            _profissional = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanConfirm));
            OnPropertyChanged(nameof(SelectedSummary));
        }
    }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            _selectedDate = value;
            OnPropertyChanged();
            _ = LoadDisponibilidadeAsync();
            OnPropertyChanged(nameof(CanConfirm));
            OnPropertyChanged(nameof(SelectedSummary));
        }
    }

    public string SelectedTime
    {
        get => _selectedTime;
        set
        {
            _selectedTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanConfirm));
            OnPropertyChanged(nameof(SelectedSummary));
        }
    }

    public DateTime MinDate => DateTime.Today;
    public DateTime MaxDate => DateTime.Today.AddDays(30);

    public bool CanConfirm => Profissional != null && !string.IsNullOrWhiteSpace(SelectedTime) && SelectedDate >= MinDate;
    public bool HasSelection => !string.IsNullOrWhiteSpace(SelectedTime);
    public string SelectedSummary => HasSelection
        ? $"Selecionado: {SelectedDate:ddd, dd/MM} às {SelectedTime}"
        : "Selecione data e horário";

    private async Task LoadDisponibilidadeAsync()
    {
        try
        {
            AvailableTimes.Clear();
            if (Profissional == null) return;
            var slots = await _agendamentoService.GetDisponibilidadeAsync(Profissional.Id, SelectedDate);
            foreach (var s in slots)
                AvailableTimes.Add(s);
            // Resetar seleção se horário atual não estiver disponível
            if (!string.IsNullOrEmpty(SelectedTime) && !AvailableTimes.Contains(SelectedTime))
                SelectedTime = string.Empty;
            OnPropertyChanged(nameof(AvailableTimes));
        }
        catch
        {
            // fallback simples
            AvailableTimes.Clear();
            for (int h = 9; h <= 17; h++) AvailableTimes.Add($"{h:00}:00");
            OnPropertyChanged(nameof(AvailableTimes));
        }
    }

    private async void OnConfirm()
    {
        await Confirm();
    }

    private async void OnCancel()
    {
        await Cancel();
    }

    private async Task Confirm()
    {
        if (Profissional == null)
        {
            await Shell.Current.DisplayAlert("Erro", "Nenhum profissional selecionado.", "OK");
            return;
        }

        if (string.IsNullOrEmpty(SelectedTime))
        {
            await Shell.Current.DisplayAlert("Erro", "Selecione um horário.", "OK");
            return;
        }

        var (ok, error) = await _agendamentoService.CriarAgendamentoAsync(Profissional.Id, SelectedDate, SelectedTime);
        if (!ok)
        {
            await Shell.Current.DisplayAlert("Erro", $"Não foi possível criar o agendamento. {error}", "OK");
            return;
        }

        var message = $"Agendado com {Profissional.NomeCompleto}\n{SelectedDate:dd/MM/yyyy} às {SelectedTime}";
        await Shell.Current.DisplayAlert("Agendamento", message, "OK");
        await Shell.Current.GoToAsync("..");
    }

    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Profissional", out var profObj) && profObj is Profissional p)
        {
            Profissional = p;
            _ = LoadDisponibilidadeAsync();
        }
        else if (query.TryGetValue("profissional", out var profObj2) && profObj2 is Profissional p2)
        {
            Profissional = p2;
            _ = LoadDisponibilidadeAsync();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
