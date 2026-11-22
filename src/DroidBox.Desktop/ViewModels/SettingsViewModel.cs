using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidBox.Desktop.Services;
using DroidBox.Shared.Helpers;

namespace DroidBox.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private bool _isDarkTheme;

    [ObservableProperty]
    private string _sdkDirectory;

    [ObservableProperty]
    private string _dataDirectory;

    [ObservableProperty]
    private int _ramMb = 2048;

    [ObservableProperty]
    private string _settingsStatusMessage = string.Empty;

    public SettingsViewModel(IThemeService themeService)
    {
        _themeService = themeService;
        _isDarkTheme = themeService.IsDarkTheme;
        _sdkDirectory = PathHelper.GetSdkDirectory();
        _dataDirectory = PathHelper.GetDataDirectory();
    }

    [RelayCommand]
    public void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        _themeService.SetTheme(IsDarkTheme);
    }

    [RelayCommand]
    public void ResetToDefaultSdkDirectory()
    {
        var defaultDir = PathHelper.GetDefaultRecommendedSdkDirectory();
        PathHelper.SetCustomSdkDirectory(null);
        SdkDirectory = defaultDir;
        SettingsStatusMessage = "SDK path reset to default recommended directory.";
    }

    [RelayCommand]
    public void SaveCustomSdkDirectory()
    {
        if (!string.IsNullOrWhiteSpace(SdkDirectory))
        {
            PathHelper.SetCustomSdkDirectory(SdkDirectory);
            SettingsStatusMessage = "Custom SDK directory saved successfully.";
        }
    }
}
