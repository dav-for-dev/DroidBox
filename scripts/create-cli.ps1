$ErrorActionPreference = "Stop"

Set-Location "E:\code-from-haris\DroidBox\src\DroidBox.Cli"

New-Item -ItemType Directory -Path "Commands" -Force | Out-Null
New-Item -ItemType Directory -Path "Services" -Force | Out-Null

# --- CliOutputService.cs ---
@'
namespace DroidBox.Cli.Services;

public static class CliOutputService
{
    public static void Success(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("[SUCCESS] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public static void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[INFO] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public static void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[WARN] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("[ERROR] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public static void Header(string title)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"=== {title} ===");
        Console.ResetColor();
    }
}
'@ | Set-Content -Path "Services\CliOutputService.cs"

# --- IpcClient.cs ---
@'
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using DroidBox.Shared.Constants;

namespace DroidBox.Cli.Services;

public record IpcMessage(string Command, Dictionary<string, string>? Arguments = null);
public record IpcResponse(bool Success, string Message, string? PayloadJson = null);

public class IpcClient
{
    public static async Task<IpcResponse> SendCommandAsync(string command, Dictionary<string, string>? args = null, int timeoutMs = 5000, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", AppConstants.PipeName, PipeDirection.InOut);
            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await client.ConnectAsync(linked.Token);

            var req = new IpcMessage(command, args);
            var json = JsonSerializer.Serialize(req);
            var bytes = Encoding.UTF8.GetBytes(json);

            await client.WriteAsync(BitConverter.GetBytes(bytes.Length), linked.Token);
            await client.WriteAsync(bytes, linked.Token);
            await client.FlushAsync(linked.Token);

            // Read response
            var lenBuffer = new byte[4];
            await client.ReadExactlyAsync(lenBuffer, linked.Token);
            var resLen = BitConverter.ToInt32(lenBuffer, 0);

            var resBuffer = new byte[resLen];
            await client.ReadExactlyAsync(resBuffer, linked.Token);
            var resJson = Encoding.UTF8.GetString(resBuffer);

            return JsonSerializer.Deserialize<IpcResponse>(resJson) ?? new IpcResponse(false, "Invalid response from service");
        }
        catch (TimeoutException)
        {
            return new IpcResponse(false, "Timeout connecting to DroidBox service");
        }
        catch (Exception ex)
        {
            return new IpcResponse(false, $"IPC communication error: {ex.Message}");
        }
    }
}
'@ | Set-Content -Path "Services\IpcClient.cs"

# --- COMMANDS ---
@'
using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Services;
using DroidBox.Core.Models;

namespace DroidBox.Cli.Commands;

public static class SetupCommand
{
    public static Command Build(ISdkSetupService setupService)
    {
        var cmd = new Command("setup", "Prepares and configures the complete Android virtual testing environment");

        var forceOption = new Option<bool>("--force", "Force re-download and re-configuration of Android SDK");
        cmd.AddOption(forceOption);

        cmd.SetHandler(async (force) =>
        {
            CliOutputService.Header("DroidBox - Android Environment Setup");
            CliOutputService.Info("Validating system prerequisites and downloading required runtime packages...");

            var progress = new Progress<ProgressInfo>(p =>
            {
                CliOutputService.Info($"[{p.Percentage:0}%] {p.Message}");
            });

            var result = await setupService.SetupAsync(progress);
            if (result.Success)
            {
                CliOutputService.Success("Android testing environment is fully prepared and ready for use!");
            }
            else
            {
                CliOutputService.Error(result.Message);
            }
        }, forceOption);

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\SetupCommand.cs"

@'
using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Services;

namespace DroidBox.Cli.Commands;

public static class DoctorCommand
{
    public static Command Build(ISdkSetupService setupService)
    {
        var cmd = new Command("doctor", "Inspects and diagnoses system and Android runtime health");

        cmd.SetHandler(async () =>
        {
            CliOutputService.Header("DroidBox Doctor - System Diagnostics");
            var report = await setupService.CheckHealthAsync();

            foreach (var comp in report.Components)
            {
                if (comp.IsHealthy)
                {
                    CliOutputService.Success($"{comp.Name}: {comp.Status} ({comp.Details})");
                }
                else
                {
                    CliOutputService.Warning($"{comp.Name}: {comp.Status} - {comp.Details}");
                }
            }

            if (report.IsHealthy)
            {
                CliOutputService.Success("All system components and dependencies are healthy!");
            }
            else
            {
                CliOutputService.Warning("Some recommendations or components require attention. Run 'droidbox setup' to configure missing elements.");
            }
        });

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\DoctorCommand.cs"

@'
using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Services;

namespace DroidBox.Cli.Commands;

public static class StatusCommand
{
    public static Command Build(IEmulatorService emulatorService, IDeviceService deviceService)
    {
        var cmd = new Command("status", "Displays current emulator runtime and device profile status");

        cmd.SetHandler(async () =>
        {
            CliOutputService.Header("DroidBox Status");
            var state = await emulatorService.GetStateAsync();
            var profile = await deviceService.GetActiveProfileAsync();

            CliOutputService.Info($"Emulator State: {state}");
            if (profile != null)
            {
                CliOutputService.Info($"Active Profile: {profile.DisplayName} ({profile.Width}x{profile.Height}, {profile.Dpi} DPI, {profile.Orientation})");
            }
        });

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\StatusCommand.cs"

@'
using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Services;

namespace DroidBox.Cli.Commands;

public static class InstallCommand
{
    public static Command Build(IApkService apkService, IDeviceService deviceService)
    {
        var cmd = new Command("install", "Installs an APK file into the virtual Android environment");

        var apkArg = new Argument<string>("apk", "Path to the APK file to install");
        var modeOption = new Option<string>("--mode", () => "mobile", "Target device profile: 'tablet' or 'mobile'");

        cmd.AddArgument(apkArg);
        cmd.AddOption(modeOption);

        cmd.SetHandler(async (apk, mode) =>
        {
            CliOutputService.Header($"DroidBox APK Installer - Mode: {mode}");

            if (!File.Exists(apk))
            {
                CliOutputService.Error($"APK file not found: {apk}");
                return;
            }

            // Switch profile if specified
            if (!string.IsNullOrWhiteSpace(mode))
            {
                await deviceService.SwitchProfileAsync(mode);
            }

            CliOutputService.Info($"Installing {Path.GetFileName(apk)}...");
            var result = await apkService.InstallAsync(apk);

            if (result.Success)
            {
                CliOutputService.Success(result.Message);
            }
            else
            {
                CliOutputService.Error(result.Message);
            }
        }, apkArg, modeOption);

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\InstallCommand.cs"

@'
using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Services;

namespace DroidBox.Cli.Commands;

public static class LaunchCommand
{
    public static Command Build(IApkService apkService)
    {
        var cmd = new Command("launch", "Launches an installed Android application");

        var pkgArg = new Argument<string>("package", "Package name of the application (e.g. com.example.app)");
        var activityOption = new Option<string?>("--activity", "Optional specific Activity name to launch");

        cmd.AddArgument(pkgArg);
        cmd.AddOption(activityOption);

        cmd.SetHandler(async (pkg, activity) =>
        {
            CliOutputService.Info($"Launching application: {pkg}");
            var result = await apkService.LaunchAsync(pkg, activity);

            if (result.Success)
            {
                CliOutputService.Success(result.Message);
            }
            else
            {
                CliOutputService.Error(result.Message);
            }
        }, pkgArg, activityOption);

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\LaunchCommand.cs"

@'
using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Services;

namespace DroidBox.Cli.Commands;

public static class UninstallCommand
{
    public static Command Build(IApkService apkService)
    {
        var cmd = new Command("uninstall", "Uninstalls an application from the virtual environment");

        var pkgArg = new Argument<string>("package", "Package name of the application to uninstall");
        cmd.AddArgument(pkgArg);

        cmd.SetHandler(async (pkg) =>
        {
            CliOutputService.Info($"Uninstalling package: {pkg}");
            var result = await apkService.UninstallAsync(pkg);

            if (result.Success)
            {
                CliOutputService.Success(result.Message);
            }
            else
            {
                CliOutputService.Error(result.Message);
            }
        }, pkgArg);

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\UninstallCommand.cs"

@'
using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Services;

namespace DroidBox.Cli.Commands;

public static class ListCommand
{
    public static Command Build(IApkService apkService)
    {
        var cmd = new Command("list", "Lists all installed applications in the testing environment");

        cmd.SetHandler(async () =>
        {
            CliOutputService.Header("Installed Applications");
            var apps = await apkService.GetInstalledAppsAsync();

            if (apps.Count == 0)
            {
                CliOutputService.Info("No installed applications recorded yet.");
                return;
            }

            foreach (var app in apps)
            {
                Console.WriteLine($"- {app.AppName} ({app.PackageName}) v{app.VersionName} [{app.CpuArchitecture}]");
            }
        });

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\ListCommand.cs"

@'
using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Services;

namespace DroidBox.Cli.Commands;

public static class DevicesCommand
{
    public static Command Build(IDeviceService deviceService)
    {
        var cmd = new Command("devices", "Lists available virtual device profiles");

        cmd.SetHandler(async () =>
        {
            CliOutputService.Header("Virtual Device Profiles");
            var profiles = await deviceService.GetProfilesAsync();

            foreach (var p in profiles)
            {
                var def = p.IsDefault ? " [Default]" : "";
                Console.WriteLine($"- {p.Name}: {p.DisplayName} ({p.Width}x{p.Height}, {p.Dpi} DPI, {p.Orientation}){def}");
            }
        });

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\DevicesCommand.cs"

@'
using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Services;

namespace DroidBox.Cli.Commands;

public static class DeviceCommand
{
    public static Command Build(IDeviceService deviceService)
    {
        var cmd = new Command("device", "Switches or queries the active device profile");

        var profileArg = new Argument<string>("profile", () => "", "Name of the profile ('tablet' or 'mobile')");
        cmd.AddArgument(profileArg);

        cmd.SetHandler(async (profile) =>
        {
            if (string.IsNullOrWhiteSpace(profile))
            {
                var active = await deviceService.GetActiveProfileAsync();
                CliOutputService.Info($"Active profile: {active?.DisplayName ?? "None"}");
                return;
            }

            var result = await deviceService.SwitchProfileAsync(profile);
            if (result.Success)
            {
                CliOutputService.Success(result.Message);
            }
            else
            {
                CliOutputService.Error(result.Message);
            }
        }, profileArg);

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\DeviceCommand.cs"

@'
using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Services;

namespace DroidBox.Cli.Commands;

public static class ScreenshotCommand
{
    public static Command Build(IScreenCaptureService screenService)
    {
        var cmd = new Command("screenshot", "Captures a screenshot of the virtual device screen");

        var outArg = new Argument<string>("output", () => "screenshot.png", "Output file path (.png)");
        cmd.AddArgument(outArg);

        cmd.SetHandler(async (output) =>
        {
            CliOutputService.Info($"Capturing screenshot to {output}...");
            try
            {
                await screenService.SaveScreenshotAsync(output);
                CliOutputService.Success($"Screenshot saved successfully: {output}");
            }
            catch (Exception ex)
            {
                CliOutputService.Error($"Failed to capture screenshot: {ex.Message}");
            }
        }, outArg);

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\ScreenshotCommand.cs"

@'
using System.CommandLine;
using DroidBox.Cli.Services;
using DroidBox.Shared.Helpers;

namespace DroidBox.Cli.Commands;

public static class LogsCommand
{
    public static Command Build()
    {
        var cmd = new Command("logs", "Shows recent execution and emulator logs");

        var linesOption = new Option<int>("--lines", () => 50, "Number of recent log lines to display");
        cmd.AddOption(linesOption);

        cmd.SetHandler((lines) =>
        {
            var logDir = PathHelper.GetLogsDirectory();
            if (!Directory.Exists(logDir))
            {
                CliOutputService.Info("No logs recorded yet.");
                return;
            }

            var latestLog = Directory.GetFiles(logDir, "droidbox-*.log")
                .OrderByDescending(f => f)
                .FirstOrDefault();

            if (latestLog == null || !File.Exists(latestLog))
            {
                CliOutputService.Info("No log files found.");
                return;
            }

            CliOutputService.Header($"Latest Logs ({Path.GetFileName(latestLog)})");
            var allLines = File.ReadAllLines(latestLog);
            var printLines = allLines.TakeLast(lines);
            foreach (var l in printLines)
            {
                Console.WriteLine(l);
            }
        }, linesOption);

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\LogsCommand.cs"

@'
using System.CommandLine;
using System.Diagnostics;
using DroidBox.Cli.Services;
using DroidBox.Shared.Helpers;

namespace DroidBox.Cli.Commands;

public static class DocsCommand
{
    public static Command Build()
    {
        var cmd = new Command("docs", "Opens offline documentation in the default browser");

        cmd.SetHandler(() =>
        {
            var docsDir = PathHelper.GetDocsDirectory();
            var indexPath = Path.Combine(docsDir, "index.html");

            CliOutputService.Header("DroidBox Offline Documentation");
            CliOutputService.Info($"Documentation path: {docsDir}");

            if (File.Exists(indexPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = indexPath,
                        UseShellExecute = true
                    });
                    CliOutputService.Success("Opened offline documentation in browser.");
                }
                catch (Exception ex)
                {
                    CliOutputService.Warning($"Could not auto-open browser: {ex.Message}. View manually at {indexPath}");
                }
            }
            else
            {
                CliOutputService.Info($"Docs are available at: {docsDir}");
            }
        });

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\DocsCommand.cs"

@'
using System.CommandLine;
using DroidBox.Cli.Services;

namespace DroidBox.Cli.Commands;

public static class UpdateCommand
{
    public static Command Build()
    {
        var cmd = new Command("update", "Checks for DroidBox application updates");

        cmd.SetHandler(() =>
        {
            CliOutputService.Header("DroidBox Update Checker");
            CliOutputService.Info("DroidBox v0.1.0-alpha is up to date.");
        });

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\UpdateCommand.cs"

@'
using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Services;

namespace DroidBox.Cli.Commands;

public static class StopCommand
{
    public static Command Build(IEmulatorService emulatorService)
    {
        var cmd = new Command("stop", "Stops any running Android emulator instance");

        cmd.SetHandler(async () =>
        {
            CliOutputService.Info("Stopping emulator...");
            var result = await emulatorService.StopAsync();
            if (result.Success)
            {
                CliOutputService.Success(result.Message);
            }
            else
            {
                CliOutputService.Error(result.Message);
            }
        });

        return cmd;
    }
}
'@ | Set-Content -Path "Commands\StopCommand.cs"

# --- Program.cs ---
@'
using System.CommandLine;
using DroidBox.Application;
using DroidBox.Application.Services;
using DroidBox.Cli.Commands;
using DroidBox.Infrastructure;
using DroidBox.Infrastructure.Logging;
using DroidBox.Runtime.AndroidSdk;
using DroidBox.Runtime.Qemu;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DroidBox.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        LoggingConfiguration.ConfigureLogger();

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddInfrastructureServices();
                services.AddApplicationServices();
                services.AddAndroidSdkRuntime();
                services.AddQemuRuntime();
            })
            .Build();

        var emulatorService = host.Services.GetRequiredService<IEmulatorService>();
        var apkService = host.Services.GetRequiredService<IApkService>();
        var deviceService = host.Services.GetRequiredService<IDeviceService>();
        var setupService = host.Services.GetRequiredService<ISdkSetupService>();
        var screenService = host.Services.GetRequiredService<IScreenCaptureService>();

        var root = new RootCommand("DroidBox - Lightweight Android Testing Environment for Windows");

        root.AddCommand(SetupCommand.Build(setupService));
        root.AddCommand(DoctorCommand.Build(setupService));
        root.AddCommand(StatusCommand.Build(emulatorService, deviceService));
        root.AddCommand(InstallCommand.Build(apkService, deviceService));
        root.AddCommand(LaunchCommand.Build(apkService));
        root.AddCommand(UninstallCommand.Build(apkService));
        root.AddCommand(ListCommand.Build(apkService));
        root.AddCommand(DevicesCommand.Build(deviceService));
        root.AddCommand(DeviceCommand.Build(deviceService));
        root.AddCommand(ScreenshotCommand.Build(screenService));
        root.AddCommand(LogsCommand.Build());
        root.AddCommand(DocsCommand.Build());
        root.AddCommand(UpdateCommand.Build());
        root.AddCommand(StopCommand.Build(emulatorService));

        return await root.InvokeAsync(args);
    }
}
'@ | Set-Content -Path "Program.cs"

Write-Host "DroidBox.Cli files created successfully!"
