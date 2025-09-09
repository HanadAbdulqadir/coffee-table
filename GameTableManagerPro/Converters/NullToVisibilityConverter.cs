using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameTableManagerPro.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool reverse = parameter?.ToString()?.Equals("Reverse", StringComparison.OrdinalIgnoreCase) ?? false;
        
        if (reverse)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
