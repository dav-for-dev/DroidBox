using DroidBox.Core.Enums;

namespace DroidBox.Core.Interfaces;

public interface IInputController
{
    Task SendTapAsync(int x, int y, CancellationToken cancellationToken = default);
    Task SendSwipeAsync(int x1, int y1, int x2, int y2, int durationMs = 300, CancellationToken cancellationToken = default);
    Task SendKeyAsync(AndroidKeyCode key, CancellationToken cancellationToken = default);
    Task SendTextAsync(string text, CancellationToken cancellationToken = default);
    Task PressHomeAsync(CancellationToken cancellationToken = default);
    Task PressBackAsync(CancellationToken cancellationToken = default);
    Task PressRecentAsync(CancellationToken cancellationToken = default);
}
