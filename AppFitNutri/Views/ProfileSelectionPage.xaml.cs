using AppFitNutri.ViewModel;
using AppFitNutri.Core.Services;

namespace AppFitNutri.Views;

public partial class ProfileSelectionPage : ContentPage
{
    private readonly ProfileSelectionViewModel _viewModel;
    
    public ProfileSelectionPage(IProfileService profileService)
    {
        InitializeComponent();
        _viewModel = new ProfileSelectionViewModel(profileService);
        BindingContext = _viewModel;
        Shell.SetNavBarIsVisible(this, false);
    }

    private void OnPatientSelected(object sender, EventArgs e)
    {
        SelectProfile(PatientButton); // Paciente = 4
        _viewModel.SelectProfileCommand.Execute(4);
    }

    private void OnNutritionistSelected(object sender, EventArgs e)
    {
        SelectProfile(NutritionistButton); // Nutricionista = 2
        _viewModel.SelectProfileCommand.Execute(2);
    }

    private void OnPersonalTrainerSelected(object sender, EventArgs e)
    {
        SelectProfile(PersonalTrainerButton); // PersonalTrainer = 3
        _viewModel.SelectProfileCommand.Execute(3);
    }

    private void SelectProfile(Button selectedButton)
    {
        // Reset all buttons to default state
        ResetButtonStyles();
        
        // Highlight selected button and its parent border
        selectedButton.BackgroundColor = Color.FromArgb("#E8F5E8");
        
        // Get the parent Border and update its stroke color
        if (selectedButton.Parent is Border parentBorder)
        {
            parentBorder.Stroke = Color.FromArgb("#27AE60");
            parentBorder.StrokeThickness = 3;
        }
    }

    private void ResetButtonStyles()
    {
        // Reset Patient button
        PatientButton.BackgroundColor = Colors.Transparent;
        if (PatientButton.Parent is Border patientBorder)
        {
            patientBorder.Stroke = Color.FromArgb("#E74C3C");
            patientBorder.StrokeThickness = 2;
        }
        
        // Reset Nutritionist button
        NutritionistButton.BackgroundColor = Colors.Transparent;
        if (NutritionistButton.Parent is Border nutritionistBorder)
        {
            nutritionistBorder.Stroke = Color.FromArgb("#27AE60");
            nutritionistBorder.StrokeThickness = 2;
        }
        
        // Reset Personal Trainer button
        PersonalTrainerButton.BackgroundColor = Colors.Transparent;
        if (PersonalTrainerButton.Parent is Border trainerBorder)
        {
            trainerBorder.Stroke = Color.FromArgb("#3498DB");
            trainerBorder.StrokeThickness = 2;
        }
    }
}
