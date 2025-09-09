using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameTableManagerPro.Converters;

public class SelectionToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D2D30")) // Darker background for selected
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF252526")); // Normal background
        }

        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF252526")); // Default normal background
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
