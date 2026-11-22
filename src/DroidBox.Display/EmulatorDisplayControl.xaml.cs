using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DroidBox.Core.Enums;

namespace DroidBox.Display;

public partial class EmulatorDisplayControl : UserControl
{
    public event EventHandler? BackClicked;
    public event EventHandler? HomeClicked;
    public event EventHandler? RecentClicked;
    public event EventHandler? RotateClicked;
    public event EventHandler? ScreenshotClicked;
    public event EventHandler<(int X, int Y, DroidBox.Core.Enums.TouchAction Action)>? TouchEvent;

    private int _screenWidth = 1280;
    private int _screenHeight = 800;
    private bool _isMouseDown;

    public EmulatorDisplayControl()
    {
        InitializeComponent();
    }

    public void SetFrame(WriteableBitmap bitmap)
    {
        ScreenImage.Source = bitmap;
        PlaceholderText.Visibility = Visibility.Collapsed;
    }

    public void SetDeviceDimensions(int width, int height)
    {
        _screenWidth = width;
        _screenHeight = height;
    }

    private void Screen_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _isMouseDown = true;
        var pos = e.GetPosition(ScreenContainer);
        var coords = InputMapper.MapToScreenCoordinates(pos, ScreenContainer.RenderSize, _screenWidth, _screenHeight);
        TouchEvent?.Invoke(this, (coords.AndroidX, coords.AndroidY, DroidBox.Core.Enums.TouchAction.Down));
    }

    private void Screen_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _isMouseDown = false;
        var pos = e.GetPosition(ScreenContainer);
        var coords = InputMapper.MapToScreenCoordinates(pos, ScreenContainer.RenderSize, _screenWidth, _screenHeight);
        TouchEvent?.Invoke(this, (coords.AndroidX, coords.AndroidY, DroidBox.Core.Enums.TouchAction.Up));
    }

    private void Screen_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isMouseDown)
        {
            var pos = e.GetPosition(ScreenContainer);
            var coords = InputMapper.MapToScreenCoordinates(pos, ScreenContainer.RenderSize, _screenWidth, _screenHeight);
            TouchEvent?.Invoke(this, (coords.AndroidX, coords.AndroidY, DroidBox.Core.Enums.TouchAction.Move));
        }
    }

    private void BackBtn_Click(object sender, RoutedEventArgs e) => BackClicked?.Invoke(this, EventArgs.Empty);
    private void HomeBtn_Click(object sender, RoutedEventArgs e) => HomeClicked?.Invoke(this, EventArgs.Empty);
    private void RecentBtn_Click(object sender, RoutedEventArgs e) => RecentClicked?.Invoke(this, EventArgs.Empty);
    private void RotateBtn_Click(object sender, RoutedEventArgs e) => RotateClicked?.Invoke(this, EventArgs.Empty);
    private void ScreenshotBtn_Click(object sender, RoutedEventArgs e) => ScreenshotClicked?.Invoke(this, EventArgs.Empty);
}
