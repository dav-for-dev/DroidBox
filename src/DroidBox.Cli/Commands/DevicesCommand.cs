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
