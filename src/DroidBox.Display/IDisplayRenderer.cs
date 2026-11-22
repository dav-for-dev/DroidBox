using System.Windows.Media.Imaging;
using DroidBox.Core.Enums;

namespace DroidBox.Display;

public interface IDisplayRenderer
{
    event EventHandler<WriteableBitmap>? FrameReady;
    void RenderFrame(byte[] rawFrameBytes);
    void UpdateDimensions(int width, int height, DeviceOrientation orientation);
}
