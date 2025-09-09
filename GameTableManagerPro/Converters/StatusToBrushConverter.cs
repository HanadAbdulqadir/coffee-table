using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameTableManagerPro;

public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.ToLower() switch
            {
                "online" or "success" or "good" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4CAF50")), // Green
                "offline" or "failed" or "critical" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF44336")), // Red
                "needs attention" or "warning" or "deploying" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF9800")), // Orange
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9E9E9E")) // Gray
            };
        }

        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9E9E9E")); // Default gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
