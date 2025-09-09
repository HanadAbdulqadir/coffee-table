using System;
using System.Globalization;
using System.Windows.Data;

namespace GameTableManagerPro.Converters;

public class MonitoringToggleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isMonitoringActive)
        {
            return isMonitoringActive ? "⏹️ Stop Monitoring" : "▶️ Start Monitoring";
        }

        return "▶️ Start Monitoring";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
