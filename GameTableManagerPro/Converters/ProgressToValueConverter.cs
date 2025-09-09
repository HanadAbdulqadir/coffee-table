using System;
using System.Globalization;
using System.Windows.Data;

namespace GameTableManagerPro.Converters;

public class ProgressToValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string progressText)
        {
            if (progressText.EndsWith("%") && double.TryParse(progressText.TrimEnd('%'), out double progress))
            {
                return progress;
            }
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
