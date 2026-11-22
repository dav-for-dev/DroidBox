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
