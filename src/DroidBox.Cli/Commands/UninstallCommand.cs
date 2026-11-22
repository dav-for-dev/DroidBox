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
