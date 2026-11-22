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
