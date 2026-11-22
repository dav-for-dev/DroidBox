using System.Windows;

namespace DroidBox.Desktop.Services;

public class ThemeService : IThemeService
{
    public bool IsDarkTheme { get; private set; }

    public void SetTheme(bool isDark)
    {
        IsDarkTheme = isDark;
        var themeUri = isDark
            ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
            : new Uri("Themes/LightTheme.xaml", UriKind.Relative);

        var app = System.Windows.Application.Current;
        if (app != null)
        {
            var existing = app.Resources.MergedDictionaries.FirstOrDefault(d =>
                d.Source != null && (d.Source.OriginalString.Contains("LightTheme") || d.Source.OriginalString.Contains("DarkTheme")));

            if (existing != null)
            {
                app.Resources.MergedDictionaries.Remove(existing);
            }
            app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
        }
    }
}
