using System.CommandLine;
using DroidBox.Application.Services;
using DroidBox.Cli.Services;

namespace DroidBox.Cli.Commands;

public static class ScreenshotCommand
{
    public static Command Build(IScreenCaptureService screenService)
    {
        var cmd = new Command("screenshot", "Captures a screenshot of the virtual device screen");

        var outArg = new Argument<string>("output", () => "screenshot.png", "Output file path (.png)");
        cmd.AddArgument(outArg);

        cmd.SetHandler(async (output) =>
        {
            CliOutputService.Info($"Capturing screenshot to {output}...");
            try
            {
                await screenService.SaveScreenshotAsync(output);
                CliOutputService.Success($"Screenshot saved successfully: {output}");
            }
            catch (Exception ex)
            {
                CliOutputService.Error($"Failed to capture screenshot: {ex.Message}");
            }
        }, outArg);

        return cmd;
    }
}
