$ErrorActionPreference = "Stop"

Set-Location "E:\code-from-haris\DroidBox\src\DroidBox.Desktop"

# --- THEMES ---
@'
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <SolidColorBrush x:Key="AppBackground" Color="#F3F3F3" />
    <SolidColorBrush x:Key="SidebarBackground" Color="#EAEAEA" />
    <SolidColorBrush x:Key="CardBackground" Color="#FFFFFF" />
    <SolidColorBrush x:Key="TextPrimary" Color="#1E1E1E" />
    <SolidColorBrush x:Key="TextSecondary" Color="#666666" />
    <SolidColorBrush x:Key="BorderBrushColor" Color="#D1D1D1" />
    <SolidColorBrush x:Key="AccentColor" Color="#0078D4" />
</ResourceDictionary>
'@ | Set-Content -Path "Themes\LightTheme.xaml"

@'
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <SolidColorBrush x:Key="AppBackground" Color="#202020" />
    <SolidColorBrush x:Key="SidebarBackground" Color="#181818" />
    <SolidColorBrush x:Key="CardBackground" Color="#2B2B2B" />
    <SolidColorBrush x:Key="TextPrimary" Color="#F3F3F3" />
    <SolidColorBrush x:Key="TextSecondary" Color="#AAAAAA" />
    <SolidColorBrush x:Key="BorderBrushColor" Color="#383838" />
    <SolidColorBrush x:Key="AccentColor" Color="#4CC2FF" />
</ResourceDictionary>
'@ | Set-Content -Path "Themes\DarkTheme.xaml"

@'
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Style x:Key="NavButton" TargetType="Button">
        <Setter Property="Background" Value="Transparent" />
        <Setter Property="Foreground" Value="{DynamicResource TextPrimary}" />
        <Setter Property="FontSize" Value="14" />
        <Setter Property="FontWeight" Value="SemiBold" />
        <Setter Property="Padding" Value="16,10" />
        <Setter Property="HorizontalContentAlignment" Value="Left" />
        <Setter Property="BorderThickness" Value="0" />
        <Setter Property="Cursor" Value="Hand" />
    </Style>

    <Style x:Key="Card" TargetType="Border">
        <Setter Property="Background" Value="{DynamicResource CardBackground}" />
        <Setter Property="BorderBrush" Value="{DynamicResource BorderBrushColor}" />
        <Setter Property="BorderThickness" Value="1" />
        <Setter Property="CornerRadius" Value="8" />
        <Setter Property="Padding" Value="16" />
    </Style>

    <Style x:Key="PrimaryButton" TargetType="Button">
        <Setter Property="Background" Value="{DynamicResource AccentColor}" />
        <Setter Property="Foreground" Value="White" />
        <Setter Property="FontWeight" Value="SemiBold" />
        <Setter Property="Padding" Value="16,8" />
        <Setter Property="BorderThickness" Value="0" />
        <Setter Property="Cursor" Value="Hand" />
    </Style>
</ResourceDictionary>
'@ | Set-Content -Path "Themes\SharedStyles.xaml"

# --- App.xaml ---
@'
<Application x:Class="DroidBox.Desktop.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Themes/LightTheme.xaml" />
                <ResourceDictionary Source="Themes/SharedStyles.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
'@ | Set-Content -Path "App.xaml"

# --- App.xaml.cs ---
@'
using System.Windows;
using DroidBox.Application;
using DroidBox.Desktop.Services;
using DroidBox.Desktop.ViewModels;
using DroidBox.Desktop.Views;
using DroidBox.Display;
using DroidBox.Infrastructure;
using DroidBox.Infrastructure.Data;
using DroidBox.Infrastructure.Logging;
using DroidBox.Runtime.AndroidSdk;
using DroidBox.Runtime.Qemu;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DroidBox.Desktop;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        LoggingConfiguration.ConfigureLogger();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddInfrastructureServices();
                services.AddApplicationServices();
                services.AddAndroidSdkRuntime();
                services.AddQemuRuntime();
                services.AddDisplayServices();

                // UI Services
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<IThemeService, ThemeService>();

                // ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ApplicationsViewModel>();
                services.AddTransient<EmulatorViewModel>();
                services.AddTransient<DocumentationViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Views
                services.AddSingleton<MainWindow>();
            })
            .Build();

        await _host.StartAsync();

        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DroidBoxDbContext>();
            db.Database.EnsureCreated();
        }

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
'@ | Set-Content -Path "App.xaml.cs"

# --- DashboardView.xaml ---
@'
<UserControl x:Class="DroidBox.Desktop.Views.DashboardView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:converters="clr-namespace:DroidBox.Desktop.Converters">
    <UserControl.Resources>
        <converters:EmulatorStateToColorConverter x:Key="StateToColorConverter" />
    </UserControl.Resources>
    <ScrollViewer VerticalScrollBarVisibility="Auto">
        <StackPanel Margin="24">
            <TextBlock Text="Dashboard" FontSize="26" FontWeight="Bold" Foreground="{DynamicResource TextPrimary}" Margin="0,0,0,16" />

            <!-- Stats Row -->
            <Grid Margin="0,0,0,24">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="*" />
                </Grid.ColumnDefinitions>

                <!-- Status Card -->
                <Border Grid.Column="0" Style="{StaticResource Card}" Margin="0,0,8,0">
                    <StackPanel>
                        <TextBlock Text="Emulator Status" Foreground="{DynamicResource TextSecondary}" FontSize="12" />
                        <StackPanel Orientation="Horizontal" Margin="0,8,0,0">
                            <Ellipse Width="12" Height="12" Fill="{Binding EmulatorState, Converter={StaticResource StateToColorConverter}}" Margin="0,0,8,0" VerticalAlignment="Center" />
                            <TextBlock Text="{Binding EmulatorState}" FontSize="18" FontWeight="SemiBold" Foreground="{DynamicResource TextPrimary}" />
                        </StackPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,12,0,0">
                            <Button Content="Start" Command="{Binding StartEmulatorCommand}" Style="{StaticResource PrimaryButton}" Margin="0,0,8,0" />
                            <Button Content="Stop" Command="{Binding StopEmulatorCommand}" Padding="12,6" />
                        </StackPanel>
                    </StackPanel>
                </Border>

                <!-- Profile Card -->
                <Border Grid.Column="1" Style="{StaticResource Card}" Margin="8,0">
                    <StackPanel>
                        <TextBlock Text="Active Device Profile" Foreground="{DynamicResource TextSecondary}" FontSize="12" />
                        <TextBlock Text="{Binding ActiveProfileName}" FontSize="18" FontWeight="SemiBold" Foreground="{DynamicResource TextPrimary}" Margin="0,8,0,0" />
                        <TextBlock Text="{Binding AndroidVersion}" Foreground="{DynamicResource TextSecondary}" FontSize="13" Margin="0,4,0,0" />
                    </StackPanel>
                </Border>

                <!-- Apps Count Card -->
                <Border Grid.Column="2" Style="{StaticResource Card}" Margin="8,0,0,0">
                    <StackPanel>
                        <TextBlock Text="Installed Applications" Foreground="{DynamicResource TextSecondary}" FontSize="12" />
                        <TextBlock Text="{Binding InstalledAppsCount}" FontSize="24" FontWeight="Bold" Foreground="{DynamicResource TextPrimary}" Margin="0,8,0,0" />
                        <Button Content="Quick Install APK" Command="{Binding QuickInstallApkCommand}" Style="{StaticResource PrimaryButton}" Margin="0,8,0,0" HorizontalAlignment="Left" />
                    </StackPanel>
                </Border>
            </Grid>

            <!-- Status Banner -->
            <Border Style="{StaticResource Card}" Margin="0,0,0,24" Background="#E1F0FF">
                <TextBlock Text="{Binding StatusMessage}" Foreground="#005A9E" FontWeight="SemiBold" />
            </Border>

            <!-- Recent Applications -->
            <TextBlock Text="Recently Installed Applications" FontSize="18" FontWeight="SemiBold" Foreground="{DynamicResource TextPrimary}" Margin="0,0,0,12" />
            <Border Style="{StaticResource Card}">
                <ItemsControl ItemsSource="{Binding RecentApps}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Border BorderBrush="{DynamicResource BorderBrushColor}" BorderThickness="0,0,0,1" Padding="8,8">
                                <Grid>
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="*" />
                                        <ColumnDefinition Width="Auto" />
                                    </Grid.ColumnDefinitions>
                                    <StackPanel Grid.Column="0">
                                        <TextBlock Text="{Binding AppName}" FontWeight="SemiBold" Foreground="{DynamicResource TextPrimary}" />
                                        <TextBlock Text="{Binding PackageName}" FontSize="12" Foreground="{DynamicResource TextSecondary}" />
                                    </StackPanel>
                                    <TextBlock Grid.Column="1" Text="{Binding VersionName}" Foreground="{DynamicResource TextSecondary}" VerticalAlignment="Center" />
                                </Grid>
                            </Border>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </Border>
        </StackPanel>
    </ScrollViewer>
</UserControl>
'@ | Set-Content -Path "Views\DashboardView.xaml"

# --- DashboardView.xaml.cs ---
@'
using System.Windows.Controls;

namespace DroidBox.Desktop.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }
}
'@ | Set-Content -Path "Views\DashboardView.xaml.cs"

# --- ApplicationsView.xaml ---
@'
<UserControl x:Class="DroidBox.Desktop.Views.ApplicationsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             AllowDrop="True"
             Drop="ApplicationsView_Drop"
             DragOver="ApplicationsView_DragOver">
    <Grid Margin="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- Title & Action Bar -->
        <Grid Grid.Row="0" Margin="0,0,0,16">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <TextBlock Grid.Column="0" Text="APK Application Manager" FontSize="26" FontWeight="Bold" Foreground="{DynamicResource TextPrimary}" />
            <StackPanel Grid.Column="1" Orientation="Horizontal">
                <Button Content="Install APK" Command="{Binding InstallApkCommand}" Style="{StaticResource PrimaryButton}" Margin="0,0,8,0" />
                <Button Content="Refresh" Command="{Binding RefreshAppsCommand}" Padding="12,6" />
            </StackPanel>
        </Grid>

        <!-- Drag and drop zone -->
        <Border Grid.Row="1" Style="{StaticResource Card}" Background="#F8F9FA" BorderBrush="{DynamicResource AccentColor}" BorderThickness="1" Margin="0,0,0,16" Padding="20">
            <TextBlock Text="Drag and Drop APK files here to install directly into the emulator" Foreground="{DynamicResource TextSecondary}" HorizontalAlignment="Center" FontSize="14" FontWeight="SemiBold" />
        </Border>

        <!-- Applications DataGrid -->
        <Border Grid.Row="2" Style="{StaticResource Card}">
            <DataGrid ItemsSource="{Binding Applications}"
                      SelectedItem="{Binding SelectedApp}"
                      AutoGenerateColumns="False"
                      IsReadOnly="True"
                      HeadersVisibility="Column"
                      GridLinesVisibility="Horizontal"
                      Background="Transparent"
                      BorderThickness="0">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="Application Name" Binding="{Binding AppName}" Width="2*" />
                    <DataGridTextColumn Header="Package Name" Binding="{Binding PackageName}" Width="3*" />
                    <DataGridTextColumn Header="Version" Binding="{Binding VersionName}" Width="*" />
                    <DataGridTextColumn Header="Target SDK" Binding="{Binding TargetSdk}" Width="*" />
                    <DataGridTextColumn Header="Architecture" Binding="{Binding CpuArchitecture}" Width="*" />
                </DataGrid.Columns>
            </DataGrid>
        </Border>

        <!-- Bottom Action Bar for selected app -->
        <Grid Grid.Row="3" Margin="0,16,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <TextBlock Grid.Column="0" Text="{Binding StatusMessage}" Foreground="{DynamicResource TextSecondary}" VerticalAlignment="Center" />
            <StackPanel Grid.Column="1" Orientation="Horizontal">
                <Button Content="Launch Application" Command="{Binding LaunchSelectedAppCommand}" Style="{StaticResource PrimaryButton}" Margin="0,0,8,0" />
                <Button Content="Uninstall" Command="{Binding UninstallSelectedAppCommand}" Padding="12,6" />
            </StackPanel>
        </Grid>
    </Grid>
</UserControl>
'@ | Set-Content -Path "Views\ApplicationsView.xaml"

# --- ApplicationsView.xaml.cs ---
@'
using System.Windows;
using System.Windows.Controls;
using DroidBox.Desktop.ViewModels;

namespace DroidBox.Desktop.Views;

public partial class ApplicationsView : UserControl
{
    public ApplicationsView()
    {
        InitializeComponent();
    }

    private void ApplicationsView_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private async void ApplicationsView_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0 && DataContext is ApplicationsViewModel vm)
            {
                foreach (var file in files.Where(f => f.EndsWith(".apk", StringComparison.OrdinalIgnoreCase)))
                {
                    await vm.InstallFromPathAsync(file);
                }
            }
        }
    }
}
'@ | Set-Content -Path "Views\ApplicationsView.xaml.cs"

# --- EmulatorView.xaml ---
@'
<UserControl x:Class="DroidBox.Desktop.Views.EmulatorView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:display="clr-namespace:DroidBox.Display;assembly=DroidBox.Display">
    <Grid Margin="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <!-- Header controls -->
        <Grid Grid.Row="0" Margin="0,0,0,16">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <StackPanel Grid.Column="0">
                <TextBlock Text="Virtual Android Screen" FontSize="26" FontWeight="Bold" Foreground="{DynamicResource TextPrimary}" />
                <TextBlock Text="{Binding ActiveProfileName, StringFormat='Profile: {0}'}" Foreground="{DynamicResource TextSecondary}" Margin="0,4,0,0" />
            </StackPanel>
            <StackPanel Grid.Column="1" Orientation="Horizontal">
                <Button Content="Switch to Tablet" Command="{Binding SwitchToTabletCommand}" Margin="0,0,8,0" Padding="12,6" />
                <Button Content="Switch to Mobile" Command="{Binding SwitchToMobileCommand}" Margin="0,0,8,0" Padding="12,6" />
                <Button Content="Start Emulator" Command="{Binding StartCommand}" Style="{StaticResource PrimaryButton}" Margin="0,0,8,0" />
                <Button Content="Stop" Command="{Binding StopCommand}" Padding="12,6" />
            </StackPanel>
        </Grid>

        <!-- Embedded Display Control -->
        <Border Grid.Row="1" Style="{StaticResource Card}" Padding="0" ClipToBounds="True">
            <display:EmulatorDisplayControl />
        </Border>
    </Grid>
</UserControl>
'@ | Set-Content -Path "Views\EmulatorView.xaml"

# --- EmulatorView.xaml.cs ---
@'
using System.Windows.Controls;

namespace DroidBox.Desktop.Views;

public partial class EmulatorView : UserControl
{
    public EmulatorView()
    {
        InitializeComponent();
    }
}
'@ | Set-Content -Path "Views\EmulatorView.xaml.cs"

# --- DocumentationView.xaml ---
@'
<UserControl x:Class="DroidBox.Desktop.Views.DocumentationView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <ScrollViewer VerticalScrollBarVisibility="Auto">
        <StackPanel Margin="24">
            <TextBlock Text="Documentation" FontSize="26" FontWeight="Bold" Foreground="{DynamicResource TextPrimary}" Margin="0,0,0,16" />
            <Border Style="{StaticResource Card}" Margin="0,0,0,16">
                <StackPanel>
                    <TextBlock Text="Offline Documentation Package" FontSize="18" FontWeight="SemiBold" Foreground="{DynamicResource TextPrimary}" />
                    <TextBlock Text="All documentation is stored locally on your machine for 100% offline access." Foreground="{DynamicResource TextSecondary}" Margin="0,8,0,16" />
                    <TextBlock Text="{Binding DocsPath, StringFormat='Location: {0}'}" FontFamily="Consolas" Foreground="{DynamicResource TextSecondary}" />
                </StackPanel>
            </Border>
            <Border Style="{StaticResource Card}">
                <StackPanel>
                    <TextBlock Text="Quick CLI Reference" FontSize="16" FontWeight="SemiBold" Foreground="{DynamicResource TextPrimary}" Margin="0,0,0,8" />
                    <TextBlock Text="• droidbox setup - Prepares Android SDK environment" Margin="0,4" />
                    <TextBlock Text="• droidbox doctor - Validates prerequisites and health" Margin="0,4" />
                    <TextBlock Text="• droidbox install [app.apk] - Installs an APK" Margin="0,4" />
                    <TextBlock Text="• droidbox launch [package] - Launches an app" Margin="0,4" />
                    <TextBlock Text="• droidbox stop - Stops the running emulator" Margin="0,4" />
                </StackPanel>
            </Border>
        </StackPanel>
    </ScrollViewer>
</UserControl>
'@ | Set-Content -Path "Views\DocumentationView.xaml"

# --- DocumentationView.xaml.cs ---
@'
using System.Windows.Controls;

namespace DroidBox.Desktop.Views;

public partial class DocumentationView : UserControl
{
    public DocumentationView()
    {
        InitializeComponent();
    }
}
'@ | Set-Content -Path "Views\DocumentationView.xaml.cs"

# --- SettingsView.xaml ---
@'
<UserControl x:Class="DroidBox.Desktop.Views.SettingsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <ScrollViewer VerticalScrollBarVisibility="Auto">
        <StackPanel Margin="24">
            <TextBlock Text="Settings" FontSize="26" FontWeight="Bold" Foreground="{DynamicResource TextPrimary}" Margin="0,0,0,16" />

            <!-- Appearance -->
            <Border Style="{StaticResource Card}" Margin="0,0,0,16">
                <StackPanel>
                    <TextBlock Text="Appearance" FontSize="18" FontWeight="SemiBold" Foreground="{DynamicResource TextPrimary}" Margin="0,0,0,12" />
                    <Button Content="Toggle Dark / Light Theme" Command="{Binding ToggleThemeCommand}" Style="{StaticResource PrimaryButton}" HorizontalAlignment="Left" />
                </StackPanel>
            </Border>

            <!-- Directories -->
            <Border Style="{StaticResource Card}" Margin="0,0,0,16">
                <StackPanel>
                    <TextBlock Text="Storage and Paths" FontSize="18" FontWeight="SemiBold" Foreground="{DynamicResource TextPrimary}" Margin="0,0,0,12" />
                    <TextBlock Text="Android SDK Directory:" FontWeight="SemiBold" Foreground="{DynamicResource TextPrimary}" />
                    <TextBox Text="{Binding SdkDirectory, Mode=OneWay}" IsReadOnly="True" Margin="0,4,0,12" Padding="6" />
                    <TextBlock Text="User Data Directory:" FontWeight="SemiBold" Foreground="{DynamicResource TextPrimary}" />
                    <TextBox Text="{Binding DataDirectory, Mode=OneWay}" IsReadOnly="True" Margin="0,4,0,0" Padding="6" />
                </StackPanel>
            </Border>
        </StackPanel>
    </ScrollViewer>
</UserControl>
'@ | Set-Content -Path "Views\SettingsView.xaml"

# --- SettingsView.xaml.cs ---
@'
using System.Windows.Controls;

namespace DroidBox.Desktop.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }
}
'@ | Set-Content -Path "Views\SettingsView.xaml.cs"

# --- MainWindow.xaml ---
@'
<Window x:Class="DroidBox.Desktop.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:viewmodels="clr-namespace:DroidBox.Desktop.ViewModels"
        xmlns:views="clr-namespace:DroidBox.Desktop.Views"
        mc:Ignorable="d"
        Title="{Binding Title}" Height="750" Width="1150"
        WindowStartupLocation="CenterScreen"
        Background="{DynamicResource AppBackground}">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="220" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <!-- Sidebar Navigation -->
        <Border Grid.Column="0" Background="{DynamicResource SidebarBackground}" BorderBrush="{DynamicResource BorderBrushColor}" BorderThickness="0,0,1,0">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="*" />
                    <RowDefinition Height="Auto" />
                </Grid.RowDefinitions>

                <!-- Brand Header -->
                <StackPanel Grid.Row="0" Margin="20,24,20,24">
                    <TextBlock Text="DroidBox" FontSize="22" FontWeight="Bold" Foreground="{DynamicResource TextPrimary}" />
                    <TextBlock Text="Android Testing Environment" FontSize="11" Foreground="{DynamicResource TextSecondary}" Margin="0,2,0,0" />
                </StackPanel>

                <!-- Navigation Links -->
                <StackPanel Grid.Row="1">
                    <Button Content="Home" Command="{Binding NavigateToDashboardCommand}" Style="{StaticResource NavButton}" />
                    <Button Content="Applications" Command="{Binding NavigateToApplicationsCommand}" Style="{StaticResource NavButton}" />
                    <Button Content="Emulator" Command="{Binding NavigateToEmulatorCommand}" Style="{StaticResource NavButton}" />
                    <Button Content="Documentation" Command="{Binding NavigateToDocumentationCommand}" Style="{StaticResource NavButton}" />
                    <Button Content="Settings" Command="{Binding NavigateToSettingsCommand}" Style="{StaticResource NavButton}" />
                </StackPanel>

                <!-- Version Info -->
                <TextBlock Grid.Row="2" Text="v0.1.0-alpha • Apache 2.0" FontSize="11" Foreground="{DynamicResource TextSecondary}" Margin="20,16" />
            </Grid>
        </Border>

        <!-- Main Content Area -->
        <ContentControl Grid.Column="1" Content="{Binding CurrentView}">
            <ContentControl.Resources>
                <DataTemplate DataType="{x:Type viewmodels:DashboardViewModel}">
                    <views:DashboardView />
                </DataTemplate>
                <DataTemplate DataType="{x:Type viewmodels:ApplicationsViewModel}">
                    <views:ApplicationsView />
                </DataTemplate>
                <DataTemplate DataType="{x:Type viewmodels:EmulatorViewModel}">
                    <views:EmulatorView />
                </DataTemplate>
                <DataTemplate DataType="{x:Type viewmodels:DocumentationViewModel}">
                    <views:DocumentationView />
                </DataTemplate>
                <DataTemplate DataType="{x:Type viewmodels:SettingsViewModel}">
                    <views:SettingsView />
                </DataTemplate>
            </ContentControl.Resources>
        </ContentControl>
    </Grid>
</Window>
'@ | Set-Content -Path "Views\MainWindow.xaml"

# --- MainWindow.xaml.cs ---
@'
using System.Windows;
using DroidBox.Desktop.ViewModels;

namespace DroidBox.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
'@ | Set-Content -Path "Views\MainWindow.xaml.cs"

Write-Host "All Desktop Views created successfully!"
