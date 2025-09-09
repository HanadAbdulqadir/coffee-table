using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameTableManagerPro.Converters;

public class MonitoringButtonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isMonitoringActive)
        {
            return isMonitoringActive 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF44336")) // Red for stop
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4CAF50")); // Green for start
        }

        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4CAF50")); // Default green
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
