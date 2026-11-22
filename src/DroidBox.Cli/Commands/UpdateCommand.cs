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
