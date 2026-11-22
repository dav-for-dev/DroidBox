using System.Windows;
using System.Windows.Controls;
using DroidBox.Desktop.ViewModels;

namespace DroidBox.Desktop.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    private void DashboardView_DragOver(object sender, DragEventArgs e)
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

    private async void DashboardView_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var apk = files?.FirstOrDefault(f => f.EndsWith(".apk", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(apk) && DataContext is DashboardViewModel vm)
            {
                await vm.InstallAndLaunchApkAsync(apk);
            }
        }
    }
}
