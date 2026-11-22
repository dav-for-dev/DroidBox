using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidBox.Application.Services;
using DroidBox.Core.Entities;
using DroidBox.Core.Enums;
using DroidBox.Core.Interfaces;
using DroidBox.Core.Models;
using DroidBox.Desktop.Services;
using DroidBox.Shared.Constants;
using DroidBox.Shared.Helpers;

namespace DroidBox.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IEmulatorService _emulatorService;
    private readonly IApkService _apkService;
    private readonly IDeviceService _deviceService;
    private readonly IDialogService _dialogService;
    private readonly ISdkSetupService _sdkSetupService;
    private readonly IAdbClient _adbClient;

    // --- First-Time Setup State ---
    [ObservableProperty]
    private bool _isSetupNeeded;

    [ObservableProperty]
    private bool _isSetupInProgress;

    [ObservableProperty]
    private double _setupProgressPercentage;

    [ObservableProperty]
    private string _setupProgressMessage = string.Empty;

    [ObservableProperty]
    private bool _acceptLicenses = true;

    [ObservableProperty]
    private bool _isSdkDetected;

    [ObservableProperty]
    private bool _isEmulatorDetected;

    [ObservableProperty]
    private bool _isMobileAvdDetected;

    [ObservableProperty]
    private bool _isTabletAvdDetected;

    [ObservableProperty]
    private bool _isVirtualizationDetected;

    [ObservableProperty]
    private string _virtualizationInfo = string.Empty;

    [ObservableProperty]
    private string _diskSpaceInfo = string.Empty;

    [ObservableProperty]
    private string _ramInfo = string.Empty;

    // --- Device 1: Mobile (DroidBox-Mobile, 412x915) ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MobileStatusText), nameof(MobileStatusColor))]
    private bool _isMobileRunning;

    public string MobileStatusText => IsMobileRunning ? "Running" : "Stopped";
    public string MobileStatusColor => IsMobileRunning ? "#107C41" : "#808080";

    // --- Device 2: Tablet (DroidBox-Tablet, 1280x800) ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TabletStatusText), nameof(TabletStatusColor))]
    private bool _isTabletRunning;

    public string TabletStatusText => IsTabletRunning ? "Running" : "Stopped";
    public string TabletStatusColor => IsTabletRunning ? "#107C41" : "#808080";

    // --- Dashboard Info ---
    [ObservableProperty]
    private int _installedAppsCount;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public ObservableCollection<InstalledApp> RecentApps { get; } = new();

    public DashboardViewModel(
        IEmulatorService emulatorService,
        IApkService apkService,
        IDeviceService deviceService,
        IDialogService dialogService,
        ISdkSetupService sdkSetupService,
        IAdbClient adbClient)
    {
        _emulatorService = emulatorService;
        _apkService = apkService;
        _deviceService = deviceService;
        _dialogService = dialogService;
        _sdkSetupService = sdkSetupService;
        _adbClient = adbClient;

        _ = InitializeDashboardAsync();
    }

    public async Task InitializeDashboardAsync()
    {
        await RefreshEnvironmentStatusAsync();
        await LoadDashboardDataAsync();
    }

    public async Task RefreshEnvironmentStatusAsync()
    {
        var status = await _sdkSetupService.DetectEnvironmentAsync();
        IsSdkDetected = status.IsSdkInstalled;
        IsEmulatorDetected = status.IsEmulatorInstalled;
        IsMobileAvdDetected = status.IsMobileAvdCreated;
        IsTabletAvdDetected = status.IsTabletAvdCreated;
        IsVirtualizationDetected = status.IsVirtualizationAvailable;
        VirtualizationInfo = status.VirtualizationStatusMessage;
        DiskSpaceInfo = $"{status.AvailableDiskSpaceGb} GB free";
        RamInfo = $"{status.TotalPhysicalMemoryGb} GB total";

        IsSetupNeeded = !status.IsFullyConfigured;

        IsMobileRunning = await _emulatorService.IsDeviceRunningAsync(SdkConstants.MobileDeviceName);
        IsTabletRunning = await _emulatorService.IsDeviceRunningAsync(SdkConstants.TabletDeviceName);
    }

    [RelayCommand]
    public async Task RunSetupAsync()
    {
        if (IsSetupInProgress) return;

        IsSetupInProgress = true;
        SetupProgressPercentage = 0;
        SetupProgressMessage = "Starting automatic Android setup...";

        var progress = new Progress<ProgressInfo>(info =>
        {
            SetupProgressPercentage = info.Percentage;
            SetupProgressMessage = info.Message;
            StatusMessage = info.Message;
        });

        var result = await _sdkSetupService.RunFullSetupAsync(AcceptLicenses, progress);
        IsSetupInProgress = false;

        if (result.Success)
        {
            StatusMessage = "Android environment setup completed successfully!";
            await RefreshEnvironmentStatusAsync();
        }
        else
        {
            StatusMessage = $"Setup failed: {result.Message}";
        }
    }

    // --- Mobile Device Actions ---
    [RelayCommand]
    public async Task StartMobileAsync()
    {
        StatusMessage = "Starting DroidBox-Mobile (412x915 Portrait)...";
        var result = await _emulatorService.StartDeviceAsync(SdkConstants.MobileDeviceName);
        StatusMessage = result.Message;
        IsMobileRunning = result.Success;
        if (result.Success) IsTabletRunning = false;
    }

    [RelayCommand]
    public async Task StopMobileAsync()
    {
        StatusMessage = "Stopping DroidBox-Mobile...";
        var result = await _emulatorService.StopDeviceAsync(SdkConstants.MobileDeviceName);
        StatusMessage = result.Message;
        IsMobileRunning = false;
    }

    [RelayCommand]
    public async Task RestartMobileAsync()
    {
        StatusMessage = "Restarting DroidBox-Mobile...";
        var result = await _emulatorService.RestartAsync(SdkConstants.MobileDeviceName);
        StatusMessage = result.Message;
        IsMobileRunning = result.Success;
    }

    [RelayCommand]
    public async Task InstallApkMobileAsync()
    {
        var apkPath = _dialogService.OpenApkFileDialog();
        if (!string.IsNullOrWhiteSpace(apkPath))
        {
            await InstallAndLaunchApkAsync(apkPath, SdkConstants.MobileDeviceName);
        }
    }

    // --- Tablet Device Actions ---
    [RelayCommand]
    public async Task StartTabletAsync()
    {
        StatusMessage = "Starting DroidBox-Tablet (1280x800 Landscape)...";
        var result = await _emulatorService.StartDeviceAsync(SdkConstants.TabletDeviceName);
        StatusMessage = result.Message;
        IsTabletRunning = result.Success;
        if (result.Success) IsMobileRunning = false;
    }

    [RelayCommand]
    public async Task StopTabletAsync()
    {
        StatusMessage = "Stopping DroidBox-Tablet...";
        var result = await _emulatorService.StopDeviceAsync(SdkConstants.TabletDeviceName);
        StatusMessage = result.Message;
        IsTabletRunning = false;
    }

    [RelayCommand]
    public async Task RestartTabletAsync()
    {
        StatusMessage = "Restarting DroidBox-Tablet...";
        var result = await _emulatorService.RestartAsync(SdkConstants.TabletDeviceName);
        StatusMessage = result.Message;
        IsTabletRunning = result.Success;
    }

    [RelayCommand]
    public async Task InstallApkTabletAsync()
    {
        var apkPath = _dialogService.OpenApkFileDialog();
        if (!string.IsNullOrWhiteSpace(apkPath))
        {
            await InstallAndLaunchApkAsync(apkPath, SdkConstants.TabletDeviceName);
        }
    }

    [RelayCommand]
    public async Task QuickInstallApkAsync()
    {
        var apkPath = _dialogService.OpenApkFileDialog();
        if (!string.IsNullOrWhiteSpace(apkPath))
        {
            var targetAvd = IsMobileRunning ? SdkConstants.MobileDeviceName : SdkConstants.TabletDeviceName;
            await InstallAndLaunchApkAsync(apkPath, targetAvd);
        }
    }

    public async Task InstallAndLaunchApkAsync(string apkPath, string? targetAvd = null)
    {
        try
        {
            var avd = targetAvd ?? (IsMobileRunning ? SdkConstants.MobileDeviceName : SdkConstants.TabletDeviceName);

            // 1. Auto-start emulator if not running
            var isRunning = await _emulatorService.IsRunningAsync();
            if (!isRunning)
            {
                StatusMessage = $"Starting {avd}...";
                var startResult = await _emulatorService.StartDeviceAsync(avd);
                if (!startResult.Success)
                {
                    StatusMessage = $"Could not start emulator: {startResult.Message}";
                    return;
                }
                if (avd == SdkConstants.MobileDeviceName) IsMobileRunning = true;
                else IsTabletRunning = true;

                StatusMessage = "Waiting for virtual Android device to finish booting...";
            }

            // 2. Wait for ADB device readiness & install APK
            StatusMessage = $"Installing {System.IO.Path.GetFileName(apkPath)}...";
            var result = await _apkService.InstallAsync(apkPath);
            StatusMessage = result.Message;

            // 3. Auto-launch the app
            if (result.Success)
            {
                await LoadDashboardDataAsync();
                var apkInfo = await _apkService.AnalyzeApkAsync(apkPath);
                if (!string.IsNullOrWhiteSpace(apkInfo.PackageName))
                {
                    StatusMessage = $"Launching {apkInfo.AppName}...";
                    await _apkService.LaunchAsync(apkInfo.PackageName);
                    StatusMessage = $"Successfully installed and launched {apkInfo.AppName}!";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Installation error: {ex.Message}";
        }
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            var apps = await _apkService.GetInstalledAppsAsync();
            RecentApps.Clear();
            foreach (var app in apps.Take(5))
            {
                RecentApps.Add(app);
            }
            InstalledAppsCount = apps.Count;
        }
        catch { }
    }
}
