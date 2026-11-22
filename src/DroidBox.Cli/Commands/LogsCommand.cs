using System.CommandLine;
using DroidBox.Cli.Services;
using DroidBox.Shared.Helpers;

namespace DroidBox.Cli.Commands;

public static class LogsCommand
{
    public static Command Build()
    {
        var cmd = new Command("logs", "Shows recent execution and emulator logs");

        var linesOption = new Option<int>("--lines", () => 50, "Number of recent log lines to display");
        cmd.AddOption(linesOption);

        cmd.SetHandler((lines) =>
        {
            var logDir = PathHelper.GetLogsDirectory();
            if (!Directory.Exists(logDir))
            {
                CliOutputService.Info("No logs recorded yet.");
                return;
            }

            var latestLog = Directory.GetFiles(logDir, "droidbox-*.log")
                .OrderByDescending(f => f)
                .FirstOrDefault();

            if (latestLog == null || !File.Exists(latestLog))
            {
                CliOutputService.Info("No log files found.");
                return;
            }

            CliOutputService.Header($"Latest Logs ({Path.GetFileName(latestLog)})");
            var allLines = File.ReadAllLines(latestLog);
            var printLines = allLines.TakeLast(lines);
            foreach (var l in printLines)
            {
                Console.WriteLine(l);
            }
        }, linesOption);

        return cmd;
    }
}
