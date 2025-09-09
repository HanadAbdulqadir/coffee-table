using System;
using System.Globalization;
using System.Windows.Data;

namespace GameTableManagerPro.Converters;

public class SaveButtonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEditing)
        {
            return isEditing ? "💾 Update" : "💾 Save";
        }

        return "Save";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
