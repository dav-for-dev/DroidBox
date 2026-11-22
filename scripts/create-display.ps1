$ErrorActionPreference = "Stop"

Set-Location "E:\code-from-haris\DroidBox\src\DroidBox.Display"

# --- DisplayConfiguration.cs ---
@'
using DroidBox.Core.Enums;

namespace DroidBox.Display;

public class DisplayConfiguration
{
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 800;
    public int Dpi { get; set; } = 213;
    public DeviceOrientation Orientation { get; set; } = DeviceOrientation.Landscape;
    public int TargetFps { get; set; } = 30;
    public bool UseGrpcStreaming { get; set; } = false;
    public bool ShowControlBar { get; set; } = true;
}
'@ | Set-Content -Path "DisplayConfiguration.cs"

# --- IDisplayRenderer.cs ---
@'
using System.Windows.Media.Imaging;
using DroidBox.Core.Enums;

namespace DroidBox.Display;

public interface IDisplayRenderer
{
    event EventHandler<WriteableBitmap>? FrameReady;
    void RenderFrame(byte[] rawFrameBytes);
    void UpdateDimensions(int width, int height, DeviceOrientation orientation);
}
'@ | Set-Content -Path "IDisplayRenderer.cs"

# --- ScreenStreamRenderer.cs ---
@'
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
'@ | Set-Content -Path "ScreenStreamRenderer.cs"

# --- InputMapper.cs ---
@'
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
'@ | Set-Content -Path "InputMapper.cs"

# --- EmulatorDisplayControl.xaml ---
@'
<UserControl x:Class="DroidBox.Display.EmulatorDisplayControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
             mc:Ignorable="d" 
             d:DesignHeight="600" d:DesignWidth="800">
    <Grid Background="#1E1E1E">
        <Grid.RowDefinitions>
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- Device Screen Display Area -->
        <Border Grid.Row="0" Margin="16" Background="#000000" CornerRadius="12"
                BorderBrush="#333333" BorderThickness="4" ClipToBounds="True">
            <Grid x:Name="ScreenContainer"
                  MouseDown="Screen_MouseDown"
                  MouseUp="Screen_MouseUp"
                  MouseMove="Screen_MouseMove">
                <Image x:Name="ScreenImage" Stretch="Uniform" />
                
                <!-- Fallback / Idle State Overlay -->
                <TextBlock x:Name="PlaceholderText"
                           Text="Android Screen Display (Offline)"
                           Foreground="#888888"
                           FontSize="18"
                           FontWeight="SemiBold"
                           HorizontalAlignment="Center"
                           VerticalAlignment="Center" />
            </Grid>
        </Border>

        <!-- Bottom Navigation & Action Bar -->
        <Border Grid.Row="1" Background="#252525" Padding="12,8" BorderBrush="#333333" BorderThickness="0,1,0,0">
            <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
                <Button x:Name="BackBtn" Content="◀ Back" Click="BackBtn_Click" Margin="4,0" Padding="12,6" Background="#333333" Foreground="White" BorderThickness="0" Cursor="Hand" />
                <Button x:Name="HomeBtn" Content="● Home" Click="HomeBtn_Click" Margin="4,0" Padding="12,6" Background="#333333" Foreground="White" BorderThickness="0" Cursor="Hand" />
                <Button x:Name="RecentBtn" Content="■ Recent" Click="RecentBtn_Click" Margin="4,0" Padding="12,6" Background="#333333" Foreground="White" BorderThickness="0" Cursor="Hand" />
                <Separator Width="16" Visibility="Hidden" />
                <Button x:Name="RotateBtn" Content="↻ Rotate" Click="RotateBtn_Click" Margin="4,0" Padding="12,6" Background="#0078D4" Foreground="White" BorderThickness="0" Cursor="Hand" />
                <Button x:Name="ScreenshotBtn" Content="📷 Screenshot" Click="ScreenshotBtn_Click" Margin="4,0" Padding="12,6" Background="#333333" Foreground="White" BorderThickness="0" Cursor="Hand" />
            </StackPanel>
        </Border>
    </Grid>
</UserControl>
'@ | Set-Content -Path "EmulatorDisplayControl.xaml"

# --- EmulatorDisplayControl.xaml.cs ---
@'
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
    public event EventHandler<(int X, int Y, TouchAction Action)>? TouchEvent;

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
        TouchEvent?.Invoke(this, (coords.AndroidX, coords.AndroidY, TouchAction.Down));
    }

    private void Screen_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _isMouseDown = false;
        var pos = e.GetPosition(ScreenContainer);
        var coords = InputMapper.MapToScreenCoordinates(pos, ScreenContainer.RenderSize, _screenWidth, _screenHeight);
        TouchEvent?.Invoke(this, (coords.AndroidX, coords.AndroidY, TouchAction.Up));
    }

    private void Screen_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isMouseDown)
        {
            var pos = e.GetPosition(ScreenContainer);
            var coords = InputMapper.MapToScreenCoordinates(pos, ScreenContainer.RenderSize, _screenWidth, _screenHeight);
            TouchEvent?.Invoke(this, (coords.AndroidX, coords.AndroidY, TouchAction.Move));
        }
    }

    private void BackBtn_Click(object sender, RoutedEventArgs e) => BackClicked?.Invoke(this, EventArgs.Empty);
    private void HomeBtn_Click(object sender, RoutedEventArgs e) => HomeClicked?.Invoke(this, EventArgs.Empty);
    private void RecentBtn_Click(object sender, RoutedEventArgs e) => RecentClicked?.Invoke(this, EventArgs.Empty);
    private void RotateBtn_Click(object sender, RoutedEventArgs e) => RotateClicked?.Invoke(this, EventArgs.Empty);
    private void ScreenshotBtn_Click(object sender, RoutedEventArgs e) => ScreenshotClicked?.Invoke(this, EventArgs.Empty);
}
'@ | Set-Content -Path "EmulatorDisplayControl.xaml.cs"

# --- DependencyInjection.cs ---
@'
using Microsoft.Extensions.DependencyInjection;

namespace DroidBox.Display;

public static class DependencyInjection
{
    public static IServiceCollection AddDisplayServices(this IServiceCollection services)
    {
        services.AddSingleton<IDisplayRenderer, ScreenStreamRenderer>();
        services.AddSingleton<DisplayConfiguration>();
        return services;
    }
}
'@ | Set-Content -Path "DependencyInjection.cs"

Write-Host "DroidBox.Display files created successfully!"
