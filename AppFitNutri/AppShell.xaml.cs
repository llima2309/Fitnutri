using AppFitNutri.Views;

namespace AppFitNutri
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(CodeVerificationPopup), typeof(CodeVerificationPopup));
            Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
            Routing.RegisterRoute(nameof(ResetPasswordPage), typeof(ResetPasswordPage));
            Routing.RegisterRoute(nameof(ProfileSelectionPage), typeof(ProfileSelectionPage));
            Routing.RegisterRoute(nameof(AgendamentosPage), typeof(AgendamentosPage));
            Routing.RegisterRoute(nameof(ListaProfissionaisPage), typeof(ListaProfissionaisPage));
            Routing.RegisterRoute(nameof(PerfilPage), typeof(PerfilPage));
            Routing.RegisterRoute(nameof(UserProfileRegistrationPage), typeof(UserProfileRegistrationPage));
            Routing.RegisterRoute(nameof(AgendamentoPage), typeof(AgendamentoPage));
            Routing.RegisterRoute(nameof(MeusAgendamentosPage), typeof(MeusAgendamentosPage));
            Routing.RegisterRoute(nameof(WorkoutChoicePage), typeof(WorkoutChoicePage));
            Routing.RegisterRoute(nameof(GymWorkoutPage), typeof(GymWorkoutPage));
            Routing.RegisterRoute(nameof(HomeWorkoutPage), typeof(HomeWorkoutPage));
            Routing.RegisterRoute(nameof(DietChoiceScreen), typeof(DietChoiceScreen));
            Routing.RegisterRoute(nameof(DietDetailPage), typeof(DietDetailPage));
            Routing.RegisterRoute(nameof(HomeNutricionistaPage), typeof(HomeNutricionistaPage));
            Routing.RegisterRoute(nameof(AgendamentosProfissionalPage), typeof(AgendamentosProfissionalPage));
            
            

        }
    }
}
