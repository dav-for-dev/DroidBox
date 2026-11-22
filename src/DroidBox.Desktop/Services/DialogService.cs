using Microsoft.Win32;

namespace DroidBox.Desktop.Services;

public class DialogService : IDialogService
{
    public string? OpenApkFileDialog()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select Android Application Package (APK)",
            Filter = "Android Package (*.apk;*.xapk)|*.apk;*.xapk|All Files (*.*)|*.*",
            Multiselect = false
        };

        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }
}
