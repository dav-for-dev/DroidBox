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
    [NotifyPropertyChangedFor(nameof(StatusColor))]
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

    public string StatusColor => EmulatorState switch
    {
        EmulatorState.Running => "#107C41",
        EmulatorState.Starting => "#FFB900",
        EmulatorState.Stopping => "#FF8C00",
        EmulatorState.Error => "#E81123",
        _ => "#808080"
    };

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
        _ = LoadDashboardDataAsync();
    }

    [RelayCommand]
    public async Task StartEmulatorAsync()
    {
        StatusMessage = "Starting emulator...";
        var result = await _emulatorService.StartAsync();
        StatusMessage = result.Message;
        if (result.Success)
        {
            EmulatorState = EmulatorState.Running;
        }
    }

    [RelayCommand]
    public async Task StopEmulatorAsync()
    {
        StatusMessage = "Stopping emulator...";
        var result = await _emulatorService.StopAsync();
        StatusMessage = result.Message;
        if (result.Success)
        {
            EmulatorState = EmulatorState.Stopped;
        }
    }

    [RelayCommand]
    public async Task QuickInstallApkAsync()
    {
        var apkPath = _dialogService.OpenApkFileDialog();
        if (!string.IsNullOrWhiteSpace(apkPath))
        {
            StatusMessage = "Installing APK...";
            var result = await _apkService.InstallAsync(apkPath);
            StatusMessage = result.Message;
            if (result.Success)
            {
                await LoadDashboardDataAsync();
            }
        }
    }

    private async Task LoadDashboardDataAsync()
    {
        var apps = await _apkService.GetInstalledAppsAsync();
        RecentApps.Clear();
        foreach (var app in apps.Take(5))
        {
            RecentApps.Add(app);
        }
        InstalledAppsCount = apps.Count;
    }
}
