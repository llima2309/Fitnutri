using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Services;
using AppFitNutri.Views;

namespace AppFitNutri.ViewModel;

public class PerfilViewModel : INotifyPropertyChanged
{
    private readonly IUserProfileService _userProfileService;
    private readonly SecureTokenStore _tokenStore;
    
    private string _userName = string.Empty;
    private string _userEmail = string.Empty;
    private string _userPhone = string.Empty;
    private string _userAge = string.Empty;
    private string _userGender = string.Empty;
    private string _appVersion = "Versão 1.0.0";

    public PerfilViewModel(IUserProfileService userProfileService, SecureTokenStore tokenStore)
    {
        _userProfileService = userProfileService;
        _tokenStore = tokenStore;
        
        LogoutCommand = new Command(OnLogout);
        EditProfileCommand = new Command(OnEditProfile);
        ChangePasswordCommand = new Command(OnChangePassword);
        NotificationsCommand = new Command(OnNotifications);
        
        LoadUserProfile();
    }

    public ICommand LogoutCommand { get; }
    public ICommand EditProfileCommand { get; }
    public ICommand ChangePasswordCommand { get; }
    public ICommand NotificationsCommand { get; }

    public string UserName
    {
        get => _userName;
        set
        {
            _userName = value;
            OnPropertyChanged();
        }
    }

    public string UserEmail
    {
        get => _userEmail;
        set
        {
            _userEmail = value;
            OnPropertyChanged();
        }
    }

    public string UserPhone
    {
        get => _userPhone;
        set
        {
            _userPhone = value;
            OnPropertyChanged();
        }
    }

    public string UserAge
    {
        get => _userAge;
        set
        {
            _userAge = value;
            OnPropertyChanged();
        }
    }

    public string UserGender
    {
        get => _userGender;
        set
        {
            _userGender = value;
            OnPropertyChanged();
        }
    }

    public string AppVersion
    {
        get => _appVersion;
        set
        {
            _appVersion = value;
            OnPropertyChanged();
        }
    }

    private async void LoadUserProfile()
    {
        try
        {
            var profile = await _userProfileService.GetProfileAsync();
            
            if (profile != null)
            {
                UserName = profile.NomeCompleto;
                UserEmail = "Email do usuário"; // Por enquanto valor fixo
                UserPhone = profile.Telefone ?? "Não informado";
                UserAge = CalculateAge(profile.DataNascimento).ToString() + " anos";
                UserGender = GetGeneroDisplayName(profile.Genero);
            }
            else
            {
                // Valores padrão caso não consiga carregar o perfil
                UserName = "Usuário";
                UserEmail = "Email do usuário";
                UserPhone = "Não informado";
                UserAge = "Não informado";
                UserGender = "Não informado";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading user profile: {ex.Message}");
            
            // Valores padrão em caso de erro
            UserName = "Usuário";
            UserEmail = "Email do usuário";
            UserPhone = "Não informado";
            UserAge = "Não informado";
            UserGender = "Não informado";
        }
    }

    private static string GetGeneroDisplayName(Core.Models.Genero genero) => genero switch
    {
        Core.Models.Genero.Masculino => "Masculino",
        Core.Models.Genero.Feminino => "Feminino",
        Core.Models.Genero.Outro => "Outro",
        _ => "Não informado"
    };

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    private async void OnLogout()
    {
        try
        {
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage == null) return;
            
            bool confirm = await currentPage.DisplayAlert(
                "Sair", 
                "Tem certeza que deseja sair do aplicativo?", 
                "Sim", 
                "Não");
                
            if (confirm)
            {
                // Limpar tokens e dados salvos
                await _tokenStore.ClearAsync();
                
                // Navegar para a página de login
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnLogout: {ex.Message}");
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage != null)
                await currentPage.DisplayAlert("Erro", "Erro ao fazer logout", "OK");
        }
    }

    private async void OnEditProfile()
    {
        try
        {
            // Navegar para a página de cadastro/edição
            // A lógica de modo será determinada automaticamente no UserProfileRegistrationViewModel
            await Shell.Current.GoToAsync(nameof(UserProfileRegistrationPage));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnEditProfile: {ex.Message}");
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage != null)
                await currentPage.DisplayAlert("Erro", "Erro ao abrir edição de perfil", "OK");
        }
    }

    private async void OnChangePassword()
    {
        try
        {
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage != null)
                await currentPage.DisplayAlert("Alterar Senha", "Funcionalidade em desenvolvimento", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnChangePassword: {ex.Message}");
        }
    }

    private async void OnNotifications()
    {
        try
        {
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage != null)
                await currentPage.DisplayAlert("Notificações", "Funcionalidade em desenvolvimento", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnNotifications: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
