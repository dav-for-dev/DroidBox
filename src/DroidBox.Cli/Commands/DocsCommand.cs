using System.CommandLine;
using System.Diagnostics;
using DroidBox.Cli.Services;
using DroidBox.Shared.Helpers;

namespace DroidBox.Cli.Commands;

public static class DocsCommand
{
    public static Command Build()
    {
        var cmd = new Command("docs", "Opens offline documentation in the default browser");

        cmd.SetHandler(() =>
        {
            var docsDir = PathHelper.GetDocsDirectory();
            var indexPath = Path.Combine(docsDir, "index.html");

            CliOutputService.Header("DroidBox Offline Documentation");
            CliOutputService.Info($"Documentation path: {docsDir}");

            if (File.Exists(indexPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = indexPath,
                        UseShellExecute = true
                    });
                    CliOutputService.Success("Opened offline documentation in browser.");
                }
                catch (Exception ex)
                {
                    CliOutputService.Warning($"Could not auto-open browser: {ex.Message}. View manually at {indexPath}");
                }
            }
            else
            {
                CliOutputService.Info($"Docs are available at: {docsDir}");
            }
        });

        return cmd;
    }
}
