using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameTableManagerPro.Converters;

public class HealthStatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ViewModels.HealthStatus status)
        {
            return status switch
            {
                ViewModels.HealthStatus.Healthy => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4CAF50")), // Green
                ViewModels.HealthStatus.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF9800")), // Orange
                ViewModels.HealthStatus.Error => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF44336")), // Red
                ViewModels.HealthStatus.Info => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2196F3")), // Blue
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
