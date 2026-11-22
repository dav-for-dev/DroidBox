using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DroidBox.Core.Enums;
using Microsoft.Extensions.Logging;

namespace DroidBox.Display;

public class ScreenStreamRenderer : IDisplayRenderer
{
    private readonly ILogger<ScreenStreamRenderer> _logger;
    private int _width = 1280;
    private int _height = 800;
    private DeviceOrientation _orientation = DeviceOrientation.Landscape;

    public event EventHandler<WriteableBitmap>? FrameReady;

    public ScreenStreamRenderer(ILogger<ScreenStreamRenderer> logger)
    {
        _logger = logger;
    }

    public void UpdateDimensions(int width, int height, DeviceOrientation orientation)
    {
        _width = width;
        _height = height;
        _orientation = orientation;
    }

    public void RenderFrame(byte[] rawFrameBytes)
    {
        if (rawFrameBytes == null || rawFrameBytes.Length == 0) return;

        try
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                using var ms = new MemoryStream(rawFrameBytes);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                var writeable = new WriteableBitmap(bitmapImage);
                FrameReady?.Invoke(this, writeable);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render screen frame");
        }
    }
}
