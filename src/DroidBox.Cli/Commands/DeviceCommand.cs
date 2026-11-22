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
