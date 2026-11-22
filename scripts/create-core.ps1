$ErrorActionPreference = "Stop"

Set-Location "E:\code-from-haris\DroidBox\src\DroidBox.Core"

New-Item -ItemType Directory -Path "Enums" -Force | Out-Null
New-Item -ItemType Directory -Path "Models" -Force | Out-Null
New-Item -ItemType Directory -Path "Entities" -Force | Out-Null
New-Item -ItemType Directory -Path "Interfaces" -Force | Out-Null
New-Item -ItemType Directory -Path "Events" -Force | Out-Null

# --- ENUMS ---
@'
namespace DroidBox.Core.Enums;

public enum EmulatorState
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Error
}
'@ | Set-Content -Path "Enums\EmulatorState.cs"

@'
namespace DroidBox.Core.Enums;

public enum DeviceOrientation
{
    Portrait,
    Landscape
}
'@ | Set-Content -Path "Enums\DeviceOrientation.cs"

@'
namespace DroidBox.Core.Enums;

public enum DeviceType
{
    Mobile,
    Tablet,
    Custom
}
'@ | Set-Content -Path "Enums\DeviceType.cs"

@'
namespace DroidBox.Core.Enums;

public enum RuntimeProviderType
{
    AndroidSdk,
    Qemu
}
'@ | Set-Content -Path "Enums\RuntimeProviderType.cs"

@'
namespace DroidBox.Core.Enums;

public enum InstallationMode
{
    Full,
    CliOnly,
    Minimal
}
'@ | Set-Content -Path "Enums\InstallationMode.cs"

@'
namespace DroidBox.Core.Enums;

public enum ApkInstallResult
{
    Success,
    AlreadyInstalled,
    IncompatibleDevice,
    InstallFailed,
    InsufficientStorage,
    InvalidApk
}
'@ | Set-Content -Path "Enums\ApkInstallResult.cs"

@'
namespace DroidBox.Core.Enums;

public enum ApkAction
{
    Install,
    Update,
    Uninstall
}
'@ | Set-Content -Path "Enums\ApkAction.cs"

@'
namespace DroidBox.Core.Enums;

public enum TouchAction
{
    Down,
    Up,
    Move
}
'@ | Set-Content -Path "Enums\TouchAction.cs"

@'
namespace DroidBox.Core.Enums;

public enum AndroidKeyCode
{
    Home = 3,
    Back = 4,
    Call = 5,
    EndCall = 6,
    VolumeUp = 24,
    VolumeDown = 25,
    Power = 26,
    Camera = 27,
    Clear = 28,
    Enter = 66,
    Delete = 67,
    Notification = 83,
    Menu = 82,
    AppSwitch = 187
}
'@ | Set-Content -Path "Enums\AndroidKeyCode.cs"

# --- MODELS ---
@'
namespace DroidBox.Core.Models;

public record OperationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ErrorCode { get; init; }

    public static OperationResult Ok(string message = "") =>
        new() { Success = true, Message = message };

    public static OperationResult Fail(string message, string? errorCode = null) =>
        new() { Success = false, Message = message, ErrorCode = errorCode };
}

public record OperationResult<T> : OperationResult
{
    public T? Data { get; init; }

    public static OperationResult<T> Ok(T data, string message = "") =>
        new() { Success = true, Data = data, Message = message };

    public new static OperationResult<T> Fail(string message, string? errorCode = null) =>
        new() { Success = false, Message = message, ErrorCode = errorCode };
}
'@ | Set-Content -Path "Models\OperationResult.cs"

@'
namespace DroidBox.Core.Models;

public record ProgressInfo
{
    public double Percentage { get; init; }
    public string Message { get; init; } = string.Empty;
    public int CurrentStep { get; init; }
    public int TotalSteps { get; init; }
}
'@ | Set-Content -Path "Models\ProgressInfo.cs"

@'
namespace DroidBox.Core.Models;

public record HealthComponent
{
    public string Name { get; init; } = string.Empty;
    public bool IsHealthy { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Details { get; init; }
}

public record HealthReport
{
    public bool IsHealthy => Components.All(c => c.IsHealthy);
    public IReadOnlyList<HealthComponent> Components { get; init; } = Array.Empty<HealthComponent>();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
'@ | Set-Content -Path "Models\HealthReport.cs"

@'
namespace DroidBox.Core.Models;

public record RuntimeCapabilities
{
    public bool SupportsGpu { get; init; }
    public bool SupportsAudio { get; init; }
    public bool SupportsCamera { get; init; }
    public bool SupportsSensors { get; init; }
    public bool SupportsMultiTouch { get; init; }
    public int MaxRamMb { get; init; }
    public IReadOnlyList<string> SupportedAbis { get; init; } = Array.Empty<string>();
}
'@ | Set-Content -Path "Models\RuntimeCapabilities.cs"

# --- ENTITIES ---
@'
namespace DroidBox.Core.Entities;

public class InstalledApp
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PackageName { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string VersionName { get; set; } = string.Empty;
    public int VersionCode { get; set; }
    public int MinSdk { get; set; }
    public int TargetSdk { get; set; }
    public string CpuArchitecture { get; set; } = string.Empty;
    public string InstallPath { get; set; } = string.Empty;
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? IconPath { get; set; }
    public bool IsSystemApp { get; set; }
}
'@ | Set-Content -Path "Entities\InstalledApp.cs"

@'
namespace DroidBox.Core.Entities;

public class ApkInfo
{
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string VersionName { get; set; } = string.Empty;
    public int VersionCode { get; set; }
    public int MinSdkVersion { get; set; }
    public int TargetSdkVersion { get; set; }
    public List<string> Permissions { get; set; } = new();
    public List<string> SupportedAbis { get; set; } = new();
    public bool IsSplitApk { get; set; }
    public string? SplitName { get; set; }
}
'@ | Set-Content -Path "Entities\ApkInfo.cs"

@'
using DroidBox.Core.Enums;

namespace DroidBox.Core.Entities;

public class DeviceProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int Dpi { get; set; }
    public DeviceOrientation Orientation { get; set; }
    public DeviceType DeviceType { get; set; }
    public bool IsDefault { get; set; }
    public bool IsCustom { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static DeviceProfile CreateTablet() => new()
    {
        Name = "tablet",
        DisplayName = "Standard Tablet (1280x800)",
        Width = 1280,
        Height = 800,
        Dpi = 213,
        Orientation = DeviceOrientation.Landscape,
        DeviceType = DeviceType.Tablet,
        IsDefault = true
    };

    public static DeviceProfile CreateMobile() => new()
    {
        Name = "mobile",
        DisplayName = "Standard Mobile (412x915)",
        Width = 412,
        Height = 915,
        Dpi = 420,
        Orientation = DeviceOrientation.Portrait,
        DeviceType = DeviceType.Mobile,
        IsDefault = false
    };
}
'@ | Set-Content -Path "Entities\DeviceProfile.cs"

@'
using DroidBox.Core.Enums;

namespace DroidBox.Core.Entities;

public class EmulatorInstance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AvdName { get; set; } = string.Empty;
    public EmulatorState State { get; set; } = EmulatorState.Stopped;
    public string AndroidVersion { get; set; } = "14.0";
    public int ApiLevel { get; set; } = 34;
    public string SystemImagePath { get; set; } = string.Empty;
    public int RamMb { get; set; } = 2048;
    public DateTime? StartedAt { get; set; }
    public int? Pid { get; set; }
}
'@ | Set-Content -Path "Entities\EmulatorInstance.cs"

@'
namespace DroidBox.Core.Entities;

public class SdkPackage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PackageName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string InstallPath { get; set; } = string.Empty;
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
    public bool IsRequired { get; set; }
}
'@ | Set-Content -Path "Entities\SdkPackage.cs"

@'
namespace DroidBox.Core.Entities;

public class AppSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Description { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}
'@ | Set-Content -Path "Entities\AppSetting.cs"

@'
using DroidBox.Core.Enums;

namespace DroidBox.Core.Entities;

public class ApkHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PackageName { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string VersionName { get; set; } = string.Empty;
    public string ApkPath { get; set; } = string.Empty;
    public ApkAction Action { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
'@ | Set-Content -Path "Entities\ApkHistory.cs"

# --- INTERFACES ---
@'
using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IRuntimeProvider
{
    string Name { get; }
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    Task InitializeAsync(CancellationToken cancellationToken = default);
    RuntimeCapabilities GetCapabilities();
}
'@ | Set-Content -Path "Interfaces\IRuntimeProvider.cs"

@'
using DroidBox.Core.Enums;

namespace DroidBox.Core.Interfaces;

public interface IAndroidRuntime
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task<EmulatorState> GetStateAsync(CancellationToken cancellationToken = default);
    Task<bool> IsRunningAsync(CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Interfaces\IAndroidRuntime.cs"

@'
using DroidBox.Core.Entities;
using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IRuntimeInstaller
{
    Task<OperationResult> InstallAsync(string component, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default);
    Task<bool> IsInstalledAsync(string component, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Interfaces\IRuntimeInstaller.cs"

@'
using DroidBox.Core.Entities;

namespace DroidBox.Core.Interfaces;

public interface IDeviceProfileManager
{
    Task<IReadOnlyList<DeviceProfile>> GetProfilesAsync(CancellationToken cancellationToken = default);
    Task<DeviceProfile> CreateProfileAsync(DeviceProfile profile, CancellationToken cancellationToken = default);
    Task DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task SwitchProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<DeviceProfile?> GetActiveProfileAsync(CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Interfaces\IDeviceProfileManager.cs"

@'
using DroidBox.Core.Entities;

namespace DroidBox.Core.Interfaces;

public interface IApkAnalyzer
{
    Task<ApkInfo> AnalyzeAsync(string apkPath, CancellationToken cancellationToken = default);
    Task<byte[]?> ExtractIconAsync(string apkPath, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetPermissionsAsync(string apkPath, CancellationToken cancellationToken = default);
    Task<bool> ValidateAsync(string apkPath, CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Interfaces\IApkAnalyzer.cs"

@'
using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IAdbClient
{
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> InstallApkAsync(string apkPath, CancellationToken cancellationToken = default);
    Task<OperationResult> UninstallAsync(string packageName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListPackagesAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> LaunchActivityAsync(string packageName, string? activity = null, CancellationToken cancellationToken = default);
    Task<string> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Interfaces\IAdbClient.cs"

@'
namespace DroidBox.Core.Interfaces;

public interface IScreenshotService
{
    Task<byte[]> CaptureAsync(CancellationToken cancellationToken = default);
    Task StartStreamingAsync(Action<byte[]> onFrame, CancellationToken cancellationToken = default);
    Task StopStreamingAsync(CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Interfaces\IScreenshotService.cs"

@'
using DroidBox.Core.Enums;

namespace DroidBox.Core.Interfaces;

public interface IDisplayAdapter
{
    Task AttachAsync(CancellationToken cancellationToken = default);
    Task DetachAsync(CancellationToken cancellationToken = default);
    Task SendTouchEventAsync(int x, int y, TouchAction action, CancellationToken cancellationToken = default);
    Task SendKeyEventAsync(int keyCode, CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Interfaces\IDisplayAdapter.cs"

@'
using DroidBox.Core.Enums;

namespace DroidBox.Core.Interfaces;

public interface IInputController
{
    Task SendTapAsync(int x, int y, CancellationToken cancellationToken = default);
    Task SendSwipeAsync(int x1, int y1, int x2, int y2, int durationMs = 300, CancellationToken cancellationToken = default);
    Task SendKeyAsync(AndroidKeyCode key, CancellationToken cancellationToken = default);
    Task SendTextAsync(string text, CancellationToken cancellationToken = default);
    Task PressHomeAsync(CancellationToken cancellationToken = default);
    Task PressBackAsync(CancellationToken cancellationToken = default);
    Task PressRecentAsync(CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Interfaces\IInputController.cs"

@'
using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IRuntimeHealthChecker
{
    Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default);
    Task<IDictionary<string, string>> GetDiagnosticsAsync(CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Interfaces\IRuntimeHealthChecker.cs"

@'
using DroidBox.Core.Entities;

namespace DroidBox.Core.Interfaces;

public interface IAppRepository
{
    Task<InstalledApp?> GetByPackageNameAsync(string packageName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstalledApp>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(InstalledApp app, CancellationToken cancellationToken = default);
    Task UpdateAsync(InstalledApp app, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Interfaces\IAppRepository.cs"

@'
using DroidBox.Core.Entities;

namespace DroidBox.Core.Interfaces;

public interface IDeviceProfileRepository
{
    Task<DeviceProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DeviceProfile?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceProfile>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(DeviceProfile profile, CancellationToken cancellationToken = default);
    Task UpdateAsync(DeviceProfile profile, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Interfaces\IDeviceProfileRepository.cs"

@'
using DroidBox.Core.Entities;

namespace DroidBox.Core.Interfaces;

public interface ISdkPackageRepository
{
    Task<IReadOnlyList<SdkPackage>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SdkPackage package, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Interfaces\ISdkPackageRepository.cs"

@'
namespace DroidBox.Core.Interfaces;

public interface ISettingsRepository
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, string value, CancellationToken cancellationToken = default);
    Task<IDictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default);
}
'@ | Set-Content -Path "Interfaces\ISettingsRepository.cs"

# --- EVENTS ---
@'
using DroidBox.Core.Enums;

namespace DroidBox.Core.Events;

public record EmulatorStateChangedEvent(EmulatorState PreviousState, EmulatorState NewState, DateTime Timestamp);
'@ | Set-Content -Path "Events\EmulatorStateChangedEvent.cs"

@'
using DroidBox.Core.Entities;

namespace DroidBox.Core.Events;

public record ApkInstalledEvent(ApkInfo ApkInfo, DateTime Timestamp);
'@ | Set-Content -Path "Events\ApkInstalledEvent.cs"

@'
namespace DroidBox.Core.Events;

public record RuntimeErrorEvent(string Message, Exception? Exception, DateTime Timestamp);
'@ | Set-Content -Path "Events\RuntimeErrorEvent.cs"

Write-Host "DroidBox.Core files created successfully!"
