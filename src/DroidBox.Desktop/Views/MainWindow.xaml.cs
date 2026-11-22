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
