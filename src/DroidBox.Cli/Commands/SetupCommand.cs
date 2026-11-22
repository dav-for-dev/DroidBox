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
