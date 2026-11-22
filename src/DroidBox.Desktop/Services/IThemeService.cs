namespace DroidBox.Desktop.Services;

public interface IThemeService
{
    bool IsDarkTheme { get; }
    void SetTheme(bool isDark);
}
