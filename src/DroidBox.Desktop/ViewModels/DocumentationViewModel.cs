using CommunityToolkit.Mvvm.ComponentModel;
using DroidBox.Shared.Helpers;

namespace DroidBox.Desktop.ViewModels;

public partial class DocumentationViewModel : ObservableObject
{
    [ObservableProperty]
    private string _docsPath;

    [ObservableProperty]
    private string _docContent = "Offline Documentation for DroidBox";

    public DocumentationViewModel()
    {
        _docsPath = PathHelper.GetDocsDirectory();
    }
}
