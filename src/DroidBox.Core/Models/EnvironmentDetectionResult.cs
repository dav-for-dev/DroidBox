namespace DroidBox.Core.Models;

public record EnvironmentDetectionResult
{
    public bool IsSdkInstalled { get; init; }
    public string? SdkPath { get; init; }

    public bool IsAdbInstalled { get; init; }
    public string? AdbPath { get; init; }

    public bool IsEmulatorInstalled { get; init; }
    public string? EmulatorPath { get; init; }

    public bool AreSystemImagesInstalled { get; init; }
    public IReadOnlyList<string> InstalledSystemImages { get; init; } = Array.Empty<string>();

    public bool IsVirtualizationAvailable { get; init; }
    public string VirtualizationStatusMessage { get; init; } = string.Empty;

    public long AvailableDiskSpaceBytes { get; init; }
    public long TotalPhysicalMemoryBytes { get; init; }

    public bool IsMobileAvdCreated { get; init; }
    public bool IsTabletAvdCreated { get; init; }

    public bool AreLicensesAccepted { get; init; }

    public IReadOnlyList<string> MissingComponents { get; init; } = Array.Empty<string>();

    public bool IsFullyConfigured =>
        IsSdkInstalled &&
        IsAdbInstalled &&
        IsEmulatorInstalled &&
        AreSystemImagesInstalled &&
        IsMobileAvdCreated &&
        IsTabletAvdCreated;

    public double AvailableDiskSpaceGb => Math.Round(AvailableDiskSpaceBytes / (1024.0 * 1024 * 1024), 1);
    public double TotalPhysicalMemoryGb => Math.Round(TotalPhysicalMemoryBytes / (1024.0 * 1024 * 1024), 1);
}
