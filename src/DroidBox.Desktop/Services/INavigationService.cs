namespace DroidBox.Desktop.Services;

public interface INavigationService
{
    object? CurrentView { get; }
    event EventHandler? CurrentViewChanged;
    void NavigateTo<TViewModel>() where TViewModel : class;
    void NavigateTo(Type viewModelType);
}
