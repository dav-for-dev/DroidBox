using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidBox.Desktop.Services;

namespace DroidBox.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _title = "DroidBox - Lightweight Android Testing Environment";

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _selectedNav = "Home";

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _navigationService.CurrentViewChanged += (_, _) => CurrentView = _navigationService.CurrentView;
        NavigateToDashboard();
    }

    [RelayCommand]
    public void NavigateToDashboard()
    {
        SelectedNav = "Home";
        _navigationService.NavigateTo<DashboardViewModel>();
    }

    [RelayCommand]
    public void NavigateToApplications()
    {
        SelectedNav = "Applications";
        _navigationService.NavigateTo<ApplicationsViewModel>();
    }

    [RelayCommand]
    public void NavigateToEmulator()
    {
        SelectedNav = "Emulator";
        _navigationService.NavigateTo<EmulatorViewModel>();
    }

    [RelayCommand]
    public void NavigateToDocumentation()
    {
        SelectedNav = "Documentation";
        _navigationService.NavigateTo<DocumentationViewModel>();
    }

    [RelayCommand]
    public void NavigateToSettings()
    {
        SelectedNav = "Settings";
        _navigationService.NavigateTo<SettingsViewModel>();
    }
}
