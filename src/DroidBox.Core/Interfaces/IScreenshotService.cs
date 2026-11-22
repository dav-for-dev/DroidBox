namespace DroidBox.Core.Interfaces;

public interface IScreenshotService
{
    Task<byte[]> CaptureAsync(CancellationToken cancellationToken = default);
    Task StartStreamingAsync(Action<byte[]> onFrame, CancellationToken cancellationToken = default);
    Task StopStreamingAsync(CancellationToken cancellationToken = default);
}
