using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameTableManagerPro.Converters;

public class AssetTypeToStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string assetType)
        {
            return assetType switch
            {
                "Game" => Application.Current.FindResource("GameTypeStyle"),
                "Emulator" => Application.Current.FindResource("EmulatorTypeStyle"),
                "Driver" => Application.Current.FindResource("DriverTypeStyle"),
                "Theme" => Application.Current.FindResource("ThemeTypeStyle"),
                "Configuration" => Application.Current.FindResource("EmulatorTypeStyle"), // Reuse emulator style
                "Script" => Application.Current.FindResource("DriverTypeStyle"), // Reuse driver style
                _ => Application.Current.FindResource("GameTypeStyle") // Default to game style
            };
        }

        return Application.Current.FindResource("GameTypeStyle"); // Default style
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
