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
