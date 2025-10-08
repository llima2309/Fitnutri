using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AppFitNutri.ViewModel;

public class HomePageViewModel : INotifyPropertyChanged
{
    private string _welcomeMessage = "Bem-vindo ao FitNutri!";

    public HomePageViewModel()
    {
        LogoutCommand = new Command(async () => await Logout());
    }

    public ICommand LogoutCommand { get; }

    public string WelcomeMessage
    {
        get => _welcomeMessage;
        set
        {
            _welcomeMessage = value;
            OnPropertyChanged();
        }
    }

    private async Task Logout()
    {
        // Implementar logout se necessário
        await Shell.Current.GoToAsync("//LoginPage");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
