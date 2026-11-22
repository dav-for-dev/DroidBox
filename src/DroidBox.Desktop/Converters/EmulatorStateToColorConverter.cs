using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DroidBox.Core.Enums;

namespace DroidBox.Desktop.Converters;

public class EmulatorStateToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EmulatorState state)
        {
            return state switch
            {
                EmulatorState.Running => new SolidColorBrush(Color.FromRgb(16, 124, 65)), // Green
                EmulatorState.Starting => new SolidColorBrush(Color.FromRgb(255, 185, 0)), // Yellow
                EmulatorState.Stopping => new SolidColorBrush(Color.FromRgb(255, 140, 0)), // Orange
                EmulatorState.Error => new SolidColorBrush(Color.FromRgb(232, 17, 35)), // Red
                _ => new SolidColorBrush(Color.FromRgb(128, 128, 128)) // Gray (Stopped)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
