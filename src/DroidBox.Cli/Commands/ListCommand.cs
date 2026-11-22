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
