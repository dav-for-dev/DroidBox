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
