using DroidBox.Core.Enums;

namespace DroidBox.Core.Interfaces;

public interface IDisplayAdapter
{
    Task AttachAsync(CancellationToken cancellationToken = default);
    Task DetachAsync(CancellationToken cancellationToken = default);
    Task SendTouchEventAsync(int x, int y, TouchAction action, CancellationToken cancellationToken = default);
    Task SendKeyEventAsync(int keyCode, CancellationToken cancellationToken = default);
}
