using System.Windows;
using System.Windows.Input;
using DroidBox.Core.Enums;

namespace DroidBox.Display;

public static class InputMapper
{
    public static (int AndroidX, int AndroidY) MapToScreenCoordinates(
        Point mousePosition,
        Size containerSize,
        int screenWidth,
        int screenHeight)
    {
        if (containerSize.Width <= 0 || containerSize.Height <= 0)
        {
            return (0, 0);
        }

        var scaleX = screenWidth / containerSize.Width;
        var scaleY = screenHeight / containerSize.Height;

        var androidX = (int)(mousePosition.X * scaleX);
        var androidY = (int)(mousePosition.Y * scaleY);

        androidX = Math.Clamp(androidX, 0, screenWidth);
        androidY = Math.Clamp(androidY, 0, screenHeight);

        return (androidX, androidY);
    }

    public static AndroidKeyCode? MapKeyToAndroid(Key key)
    {
        return key switch
        {
            Key.Back => AndroidKeyCode.Back,
            Key.Escape => AndroidKeyCode.Back,
            Key.Enter => AndroidKeyCode.Enter,
            Key.Delete => AndroidKeyCode.Delete,
            Key.Home => AndroidKeyCode.Home,
            Key.F1 => AndroidKeyCode.Menu,
            Key.VolumeUp => AndroidKeyCode.VolumeUp,
            Key.VolumeDown => AndroidKeyCode.VolumeDown,
            _ => null
        };
    }
}
