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
            
            // Associar o perfil selecionado através da API
            var perfilAssociado = await _profileService.AssociarPerfilAsync(SelectedProfileType);
            
            // Exibir mensagem de sucesso
            await Application.Current.MainPage.DisplayAlert(
                "Sucesso", 
                $"Perfil associado com sucesso! Agora complete seu cadastro.", 
                "OK");
            
            // Navegar para a página de completar cadastro após associar o perfil
            await Shell.Current.GoToAsync("//UserProfileRegistrationPage");
        }
        catch (UnauthorizedAccessException)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erro de Autenticação", 
                "Sua sessão expirou. Por favor, faça login novamente.", 
                "OK");
            
            // Navegar de volta para a tela de login
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (InvalidOperationException ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erro", 
                $"Erro ao associar perfil: {ex.Message}", 
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erro", 
                $"Erro inesperado: {ex.Message}", 
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
