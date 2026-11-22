using Microsoft.Extensions.DependencyInjection;

namespace DroidBox.Desktop.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private object? _currentView;

    public object? CurrentView => _currentView;
    public event EventHandler? CurrentViewChanged;

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
        var vm = _serviceProvider.GetRequiredService(viewModelType);
        _currentView = vm;
        CurrentViewChanged?.Invoke(this, EventArgs.Empty);
    }
}
