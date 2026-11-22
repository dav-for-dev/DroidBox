namespace DroidBox.Application.Services;

public interface IScreenCaptureService
{
    Task<byte[]> CaptureAsync(CancellationToken cancellationToken = default);
    Task<string> SaveScreenshotAsync(string outputPath, CancellationToken cancellationToken = default);
}
