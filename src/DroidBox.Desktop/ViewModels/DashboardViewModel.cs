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
