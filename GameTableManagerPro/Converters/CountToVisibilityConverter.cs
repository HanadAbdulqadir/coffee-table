using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameTableManagerPro.Converters;

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            bool reverse = parameter?.ToString()?.Equals("Reverse", StringComparison.OrdinalIgnoreCase) ?? false;
            
            if (reverse)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
