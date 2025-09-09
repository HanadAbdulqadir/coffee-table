using System;
using System.Globalization;
using System.Windows.Data;

namespace GameTableManagerPro.Converters;

public class EditModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEditing)
        {
            return isEditing ? "✏️ Edit Table" : "➕ Add New Table";
        }

        return "Add New Table";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
