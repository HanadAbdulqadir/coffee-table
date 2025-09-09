using System;
using System.Globalization;
using System.Windows.Data;

namespace GameTableManagerPro.Converters;

public class BooleanToThemeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isDarkTheme)
        {
            return isDarkTheme ? "Dark" : "Light";
        }

        return "Dark"; // Default theme
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string theme)
        {
            return theme == "Dark";
        }

        return true; // Default to dark theme
    }
}
