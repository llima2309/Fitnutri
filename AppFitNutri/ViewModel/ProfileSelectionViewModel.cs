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
        await OnContinueAsync();
    }

    private async Task OnContinueAsync()
    {
        if (SelectedProfileType == 0)
            return;

        try
        {
            IsLoading = true;

            // Chamar a API para associar o perfil
            await _profileService.AssociarPerfilAsync(SelectedProfileType);

            // Navegar para a página principal baseada no tipo de perfil
            await NavigateToMainPageAsync();
        }
        catch (Exception ex)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", 
                    $"Erro ao salvar perfil: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task NavigateToMainPageAsync()
    {
        // Navegar baseado no tipo de perfil selecionado
        switch (SelectedProfileType)
        {
            case 2: // Nutricionista
            case 3: // Personal Trainer
                // Navegar para dashboard de profissional
                await Shell.Current.GoToAsync("//MainPage");
                break;
            case 4: // Paciente
                // Navegar para dashboard de paciente
                await Shell.Current.GoToAsync("//MainPage");
                break;
            default:
                await Shell.Current.GoToAsync("//MainPage");
                break;
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
