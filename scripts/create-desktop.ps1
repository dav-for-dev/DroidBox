$ErrorActionPreference = "Stop"

Set-Location "E:\code-from-haris\DroidBox\src\DroidBox.Desktop"

New-Item -ItemType Directory -Path "ViewModels" -Force | Out-Null
New-Item -ItemType Directory -Path "Views" -Force | Out-Null
New-Item -ItemType Directory -Path "Services" -Force | Out-Null
New-Item -ItemType Directory -Path "Converters" -Force | Out-Null
New-Item -ItemType Directory -Path "Themes" -Force | Out-Null

# --- SERVICES ---
@'
namespace DroidBox.Desktop.Services;

public interface INavigationService
{
    object? CurrentView { get; }
    event EventHandler? CurrentViewChanged;
    void NavigateTo<TViewModel>() where TViewModel : class;
    void NavigateTo(Type viewModelType);
}
'@ | Set-Content -Path "Services\INavigationService.cs"

@'
using Microsoft.Extensions.DependencyInjection;

namespace DroidBox.Desktop.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private object? _currentView;

    public object? CurrentView => _currentView;
    public event EventHandler? CurrentViewChanged;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        NavigateTo(typeof(TViewModel));
    }

    public void NavigateTo(Type viewModelType)
    {
        var vm = _serviceProvider.GetRequiredService(viewModelType);
        _currentView = vm;
        CurrentViewChanged?.Invoke(this, EventArgs.Empty);
    }
}
'@ | Set-Content -Path "Services\NavigationService.cs"

@'
namespace DroidBox.Desktop.Services;

public interface IDialogService
{
    string? OpenApkFileDialog();
}
'@ | Set-Content -Path "Services\IDialogService.cs"

@'
using Microsoft.Win32;

namespace DroidBox.Desktop.Services;

public class DialogService : IDialogService
{
    public string? OpenApkFileDialog()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select Android Application Package (APK)",
            Filter = "Android Package (*.apk;*.xapk)|*.apk;*.xapk|All Files (*.*)|*.*",
            Multiselect = false
        };

        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }
}
'@ | Set-Content -Path "Services\DialogService.cs"

@'
namespace DroidBox.Desktop.Services;

public interface IThemeService
{
    bool IsDarkTheme { get; }
    void SetTheme(bool isDark);
}
'@ | Set-Content -Path "Services\IThemeService.cs"

@'
using System.Windows;

namespace DroidBox.Desktop.Services;

public class ThemeService : IThemeService
{
    public bool IsDarkTheme { get; private set; }

    public void SetTheme(bool isDark)
    {
        IsDarkTheme = isDark;
        var themeUri = isDark
            ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
            : new Uri("Themes/LightTheme.xaml", UriKind.Relative);

        var app = System.Windows.Application.Current;
        if (app != null)
        {
            var existing = app.Resources.MergedDictionaries.FirstOrDefault(d =>
                d.Source != null && (d.Source.OriginalString.Contains("LightTheme") || d.Source.OriginalString.Contains("DarkTheme")));

            if (existing != null)
            {
                app.Resources.MergedDictionaries.Remove(existing);
            }
            app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
        }
    }
}
'@ | Set-Content -Path "Services\ThemeService.cs"

# --- CONVERTERS ---
@'
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DroidBox.Desktop.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}
'@ | Set-Content -Path "Converters\BoolToVisibilityConverter.cs"

@'
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DroidBox.Core.Enums;

namespace DroidBox.Desktop.Converters;

public class EmulatorStateToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EmulatorState state)
        {
            return state switch
            {
                EmulatorState.Running => new SolidColorBrush(Color.FromRgb(16, 124, 65)), // Green
                EmulatorState.Starting => new SolidColorBrush(Color.FromRgb(255, 185, 0)), // Yellow
                EmulatorState.Stopping => new SolidColorBrush(Color.FromRgb(255, 140, 0)), // Orange
                EmulatorState.Error => new SolidColorBrush(Color.FromRgb(232, 17, 35)), // Red
                _ => new SolidColorBrush(Color.FromRgb(128, 128, 128)) // Gray (Stopped)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
'@ | Set-Content -Path "Converters\EmulatorStateToColorConverter.cs"

@'
using System.Globalization;
using System.Windows.Data;

namespace DroidBox.Desktop.Converters;

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : false;
    }
}
'@ | Set-Content -Path "Converters\InverseBoolConverter.cs"

# --- VIEWMODELS ---
@'
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
'@ | Set-Content -Path "ViewModels\MainViewModel.cs"

@'
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidBox.Application.Services;
using DroidBox.Core.Entities;
using DroidBox.Core.Enums;
using DroidBox.Desktop.Services;

namespace DroidBox.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IEmulatorService _emulatorService;
    private readonly IApkService _apkService;
    private readonly IDeviceService _deviceService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private EmulatorState _emulatorState = EmulatorState.Stopped;

    [ObservableProperty]
    private string _activeProfileName = "Tablet (1280x800)";

    [ObservableProperty]
    private int _installedAppsCount;

    [ObservableProperty]
    private string _androidVersion = "Android 14 (API 34)";

    [ObservableProperty]
    private string _ramAllocation = "2048 MB";

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public ObservableCollection<InstalledApp> RecentApps { get; } = new();

    public DashboardViewModel(
        IEmulatorService emulatorService,
        IApkService apkService,
        IDeviceService deviceService,
        IDialogService dialogService)
    {
        _emulatorService = emulatorService;
        _apkService = apkService;
        _deviceService = deviceService;
        _dialogService = dialogService;
        LoadDashboardDataAsync();
    }

    public async void LoadDashboardDataAsync()
    {
        EmulatorState = await _emulatorService.GetStateAsync();
        var profile = await _deviceService.GetActiveProfileAsync();
        if (profile != null) ActiveProfileName = profile.DisplayName;

        var apps = await _apkService.GetInstalledAppsAsync();
        InstalledAppsCount = apps.Count;
        RecentApps.Clear();
        foreach (var app in apps.Take(5))
        {
            RecentApps.Add(app);
        }
    }

    [RelayCommand]
    public async Task StartEmulatorAsync()
    {
        StatusMessage = "Starting emulator...";
        var result = await _emulatorService.StartAsync();
        EmulatorState = await _emulatorService.GetStateAsync();
        StatusMessage = result.Message;
    }

    [RelayCommand]
    public async Task StopEmulatorAsync()
    {
        StatusMessage = "Stopping emulator...";
        var result = await _emulatorService.StopAsync();
        EmulatorState = await _emulatorService.GetStateAsync();
        StatusMessage = result.Message;
    }

    [RelayCommand]
    public async Task QuickInstallApkAsync()
    {
        var path = _dialogService.OpenApkFileDialog();
        if (!string.IsNullOrWhiteSpace(path))
        {
            StatusMessage = $"Installing {System.IO.Path.GetFileName(path)}...";
            var result = await _apkService.InstallAsync(path);
            StatusMessage = result.Message;
            LoadDashboardDataAsync();
        }
    }
}
'@ | Set-Content -Path "ViewModels\DashboardViewModel.cs"

@'
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidBox.Application.Services;
using DroidBox.Core.Entities;
using DroidBox.Desktop.Services;

namespace DroidBox.Desktop.ViewModels;

public partial class ApplicationsViewModel : ObservableObject
{
    private readonly IApkService _apkService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private InstalledApp? _selectedApp;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<InstalledApp> Applications { get; } = new();

    public ApplicationsViewModel(IApkService apkService, IDialogService dialogService)
    {
        _apkService = apkService;
        _dialogService = dialogService;
        RefreshAppsAsync();
    }

    [RelayCommand]
    public async Task RefreshAppsAsync()
    {
        IsLoading = true;
        Applications.Clear();
        var apps = await _apkService.GetInstalledAppsAsync();
        foreach (var a in apps)
        {
            Applications.Add(a);
        }
        IsLoading = false;
    }

    [RelayCommand]
    public async Task InstallApkAsync()
    {
        var apkPath = _dialogService.OpenApkFileDialog();
        if (!string.IsNullOrWhiteSpace(apkPath))
        {
            await InstallFromPathAsync(apkPath);
        }
    }

    public async Task InstallFromPathAsync(string apkPath)
    {
        StatusMessage = $"Installing {System.IO.Path.GetFileName(apkPath)}...";
        var result = await _apkService.InstallAsync(apkPath);
        StatusMessage = result.Message;
        await RefreshAppsAsync();
    }

    [RelayCommand]
    public async Task LaunchSelectedAppAsync()
    {
        if (SelectedApp == null) return;
        StatusMessage = $"Launching {SelectedApp.AppName}...";
        var res = await _apkService.LaunchAsync(SelectedApp.PackageName);
        StatusMessage = res.Message;
    }

    [RelayCommand]
    public async Task UninstallSelectedAppAsync()
    {
        if (SelectedApp == null) return;
        StatusMessage = $"Uninstalling {SelectedApp.AppName}...";
        var res = await _apkService.UninstallAsync(SelectedApp.PackageName);
        StatusMessage = res.Message;
        await RefreshAppsAsync();
    }
}
'@ | Set-Content -Path "ViewModels\ApplicationsViewModel.cs"

@'
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidBox.Application.Services;
using DroidBox.Core.Entities;
using DroidBox.Core.Enums;
using DroidBox.Display;

namespace DroidBox.Desktop.ViewModels;

public partial class EmulatorViewModel : ObservableObject
{
    private readonly IEmulatorService _emulatorService;
    private readonly IDeviceService _deviceService;
    private readonly IScreenCaptureService _screenService;

    [ObservableProperty]
    private EmulatorState _state = EmulatorState.Stopped;

    [ObservableProperty]
    private string _activeProfileName = "Tablet";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public EmulatorViewModel(
        IEmulatorService emulatorService,
        IDeviceService deviceService,
        IScreenCaptureService screenService)
    {
        _emulatorService = emulatorService;
        _deviceService = deviceService;
        _screenService = screenService;
        CheckStateAsync();
    }

    public async void CheckStateAsync()
    {
        State = await _emulatorService.GetStateAsync();
        var p = await _deviceService.GetActiveProfileAsync();
        if (p != null) ActiveProfileName = p.DisplayName;
    }

    [RelayCommand]
    public async Task StartAsync()
    {
        StatusMessage = "Starting...";
        var res = await _emulatorService.StartAsync();
        State = await _emulatorService.GetStateAsync();
        StatusMessage = res.Message;
    }

    [RelayCommand]
    public async Task StopAsync()
    {
        StatusMessage = "Stopping...";
        var res = await _emulatorService.StopAsync();
        State = await _emulatorService.GetStateAsync();
        StatusMessage = res.Message;
    }

    [RelayCommand]
    public async Task SwitchToTabletAsync()
    {
        await _deviceService.SwitchProfileAsync("tablet");
        CheckStateAsync();
    }

    [RelayCommand]
    public async Task SwitchToMobileAsync()
    {
        await _deviceService.SwitchProfileAsync("mobile");
        CheckStateAsync();
    }
}
'@ | Set-Content -Path "ViewModels\EmulatorViewModel.cs"

@'
using CommunityToolkit.Mvvm.ComponentModel;
using DroidBox.Shared.Helpers;

namespace DroidBox.Desktop.ViewModels;

public partial class DocumentationViewModel : ObservableObject
{
    [ObservableProperty]
    private string _docsPath;

    [ObservableProperty]
    private string _docContent = "Offline Documentation for DroidBox";

    public DocumentationViewModel()
    {
        _docsPath = PathHelper.GetDocsDirectory();
    }
}
'@ | Set-Content -Path "ViewModels\DocumentationViewModel.cs"

@'
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
}
'@ | Set-Content -Path "ViewModels\SettingsViewModel.cs"

Write-Host "ViewModels and Services created."
