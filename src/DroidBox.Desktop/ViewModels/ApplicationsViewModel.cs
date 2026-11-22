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
    private readonly IEmulatorService _emulatorService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private InstalledApp? _selectedApp;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<InstalledApp> Applications { get; } = new();

    public ApplicationsViewModel(
        IApkService apkService,
        IEmulatorService emulatorService,
        IDialogService dialogService)
    {
        _apkService = apkService;
        _emulatorService = emulatorService;
        _dialogService = dialogService;
        _ = RefreshAppsAsync();
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
        // 1. Auto-start emulator if not running
        var isRunning = await _emulatorService.IsRunningAsync();
        if (!isRunning)
        {
            StatusMessage = "Starting virtual Android emulator...";
            var startResult = await _emulatorService.StartAsync();
            if (!startResult.Success)
            {
                StatusMessage = $"Could not start emulator: {startResult.Message}";
                return;
            }
            StatusMessage = "Waiting for virtual Android device to finish booting...";
        }

        // 2. Install APK
        StatusMessage = $"Installing {System.IO.Path.GetFileName(apkPath)}...";
        var result = await _apkService.InstallAsync(apkPath);
        StatusMessage = result.Message;

        // 3. Auto-launch the app
        if (result.Success)
        {
            await RefreshAppsAsync();
            var apkInfo = await _apkService.AnalyzeApkAsync(apkPath);
            if (!string.IsNullOrWhiteSpace(apkInfo.PackageName))
            {
                StatusMessage = $"Launching {apkInfo.AppName}...";
                await _apkService.LaunchAsync(apkInfo.PackageName);
                StatusMessage = $"Successfully installed and launched {apkInfo.AppName}!";
            }
        }
    }

    [RelayCommand]
    public async Task LaunchSelectedAppAsync()
    {
        if (SelectedApp == null) return;
        StatusMessage = $"Launching {SelectedApp.AppName}...";
        var result = await _apkService.LaunchAsync(SelectedApp.PackageName);
        StatusMessage = result.Message;
    }

    [RelayCommand]
    public async Task UninstallSelectedAppAsync()
    {
        if (SelectedApp == null) return;
        StatusMessage = $"Uninstalling {SelectedApp.AppName}...";
        var result = await _apkService.UninstallAsync(SelectedApp.PackageName);
        StatusMessage = result.Message;
        if (result.Success)
        {
            await RefreshAppsAsync();
        }
    }
}
