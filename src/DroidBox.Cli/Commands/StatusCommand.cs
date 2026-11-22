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
