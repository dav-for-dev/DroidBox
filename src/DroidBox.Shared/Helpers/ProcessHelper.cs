using System.Diagnostics;
using System.Text;

namespace DroidBox.Shared.Helpers;

public record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
    public bool Success => ExitCode == 0;
}

public static class ProcessHelper
{
    public static async Task<ProcessResult> RunProcessAsync(
        string executable,
        string arguments,
        string? workingDirectory = null,
        IDictionary<string, string>? environmentVariables = null,
        int timeoutMs = 60000,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        if (environmentVariables != null)
        {
            foreach (var kvp in environmentVariables)
            {
                startInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
            }
        }

        using var process = new Process { StartInfo = startInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null) outputBuilder.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null) errorBuilder.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeoutCts = new CancellationTokenSource(timeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await process.WaitForExitAsync(linkedCts.Token);
            return new ProcessResult(process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }
            }
            catch
            {
                // Ignore process kill failure
            }

            if (timeoutCts.IsCancellationRequested)
            {
                return new ProcessResult(-1, outputBuilder.ToString(), $"Process timed out after {timeoutMs}ms");
            }
            throw;
        }
    }
}
