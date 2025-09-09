using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameTableManagerPro.Converters;

public class StepToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ViewModels.DeploymentStep currentStep && parameter is string targetStep)
        {
            if (Enum.TryParse<ViewModels.DeploymentStep>(targetStep, out var targetStepEnum))
            {
                return currentStep == targetStepEnum 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC")) // Blue for current step
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E40")); // Dark gray for other steps
            }
        }

        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E40")); // Default dark gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
