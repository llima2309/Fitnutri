using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Core.Services;

namespace AppFitNutri.ViewModel;

public class ProfileSelectionViewModel : INotifyPropertyChanged
{
    private readonly IProfileService _profileService;
    private int _selectedProfileType;
    private bool _isLoading;
    private bool _canContinue;

    public ProfileSelectionViewModel(IProfileService profileService)
    {
        _profileService = profileService;
        SelectProfileCommand = new Command<int>(OnProfileSelected);
        ContinueCommand = new Command(OnContinueExecute, () => CanContinue);
    }

    public int SelectedProfileType
    {
        get => _selectedProfileType;
        set
        {
            _selectedProfileType = value;
            OnPropertyChanged();
            CanContinue = value > 0;
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            ((Command)ContinueCommand).ChangeCanExecute();
        }
    }

    public bool CanContinue
    {
        get => _canContinue && !IsLoading;
        set
        {
            _canContinue = value;
            OnPropertyChanged();
            ((Command)ContinueCommand).ChangeCanExecute();
        }
    }

    public ICommand SelectProfileCommand { get; }
    public ICommand ContinueCommand { get; }

    private void OnProfileSelected(int profileType)
    {
        SelectedProfileType = profileType;
    }

    private async void OnContinueExecute()
    {
        try
        {
            IsLoading = true;
            
            // Navegar diretamente para a tela de cadastro de perfil completo
            // O tipo de perfil selecionado pode ser passado como parâmetro se necessário
            await Shell.Current.GoToAsync("//UserProfileRegistrationPage");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erro", 
                $"Erro ao continuar: {ex.Message}", 
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public string GetProfileDescription(int profileType)
    {
        return profileType switch
        {
            2 => "Nutricionista - Profissional da área que quer atender pacientes",
            3 => "Personal Trainer - Educador físico que quer acompanhar alunos",
            4 => "Paciente - Busca orientação nutricional e acompanhamento",
            _ => "Selecione um perfil"
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
