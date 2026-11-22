using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Services;

namespace DroidBox.Cli.Commands;

public static class DoctorCommand
{
    public static Command Build(ISdkSetupService setupService)
    {
        var cmd = new Command("doctor", "Inspects and diagnoses system and Android runtime health");

        cmd.SetHandler(async () =>
        {
            CliOutputService.Header("DroidBox Doctor - System Diagnostics");
            var report = await setupService.CheckHealthAsync();

            foreach (var comp in report.Components)
            {
                if (comp.IsHealthy)
                {
                    CliOutputService.Success($"{comp.Name}: {comp.Status} ({comp.Details})");
                }
                else
                {
                    CliOutputService.Warning($"{comp.Name}: {comp.Status} - {comp.Details}");
                }
            }

            if (report.IsHealthy)
            {
                CliOutputService.Success("All system components and dependencies are healthy!");
            }
            else
            {
                CliOutputService.Warning("Some recommendations or components require attention. Run 'droidbox setup' to configure missing elements.");
            }
        });

        return cmd;
    }
}
