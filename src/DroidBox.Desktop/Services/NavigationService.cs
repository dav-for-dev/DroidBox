using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using DroidBox.Desktop.ViewModels;
using DroidBox.Desktop.Views;

namespace DroidBox.Desktop.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private object? _currentView;

    public object? CurrentView => _currentView;
    public event EventHandler? CurrentViewChanged;

    private readonly Dictionary<Type, Type> _viewMap = new()
    {
        { typeof(DashboardViewModel), typeof(DashboardView) },
        { typeof(ApplicationsViewModel), typeof(ApplicationsView) },
        { typeof(EmulatorViewModel), typeof(EmulatorView) },
        { typeof(DocumentationViewModel), typeof(DocumentationView) },
        { typeof(SettingsViewModel), typeof(SettingsView) }
    };

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        NavigateTo(typeof(TViewModel));
    }

    public void NavigateTo(Type viewModelType)
    {
        if (_viewMap.TryGetValue(viewModelType, out var viewType))
        {
            var view = (UserControl)_serviceProvider.GetRequiredService(viewType);
            var vm = _serviceProvider.GetRequiredService(viewModelType);
            view.DataContext = vm;
            _currentView = view;
        }
        else
        {
            _currentView = _serviceProvider.GetRequiredService(viewModelType);
        }
        CurrentViewChanged?.Invoke(this, EventArgs.Empty);
    }
}
