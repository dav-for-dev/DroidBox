using DroidBox.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DroidBox.Application.Services;

public class ScreenCaptureService : IScreenCaptureService
{
    private readonly IScreenshotService _screenshotService;
    private readonly ILogger<ScreenCaptureService> _logger;

    public ScreenCaptureService(IScreenshotService screenshotService, ILogger<ScreenCaptureService> logger)
    {
        _screenshotService = screenshotService;
        _logger = logger;
    }

    public async Task<byte[]> CaptureAsync(CancellationToken cancellationToken = default)
    {
        return await _screenshotService.CaptureAsync(cancellationToken);
    }

    public async Task<string> SaveScreenshotAsync(string outputPath, CancellationToken cancellationToken = default)
    {
        var bytes = await _screenshotService.CaptureAsync(cancellationToken);
        var dir = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await File.WriteAllBytesAsync(outputPath, bytes, cancellationToken);
        _logger.LogInformation("Saved screenshot to {Path} ({Bytes} bytes)", outputPath, bytes.Length);
        return outputPath;
    }
}
