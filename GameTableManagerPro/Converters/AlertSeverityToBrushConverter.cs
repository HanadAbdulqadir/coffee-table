using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameTableManagerPro.Converters;

public class AlertSeverityToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ViewModels.AlertSeverity severity)
        {
            return severity switch
            {
                ViewModels.AlertSeverity.Critical => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF44336")), // Red
                ViewModels.AlertSeverity.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF9800")), // Orange
                ViewModels.AlertSeverity.Info => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2196F3")), // Blue
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCCCCCC")) // Light gray
            };
        }

        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCCCCCC")); // Default light gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
