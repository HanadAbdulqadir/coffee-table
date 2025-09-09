using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameTableManagerPro.Converters;

public class LogLevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ViewModels.LogLevel level)
        {
            return level switch
            {
                ViewModels.LogLevel.Info => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCCCCCC")), // Light gray
                ViewModels.LogLevel.Success => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4CAF50")), // Green
                ViewModels.LogLevel.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF9800")), // Orange
                ViewModels.LogLevel.Error => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF44336")), // Red
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCCCCCC")) // Default light gray
            };
        }

        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCCCCCC")); // Default light gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
