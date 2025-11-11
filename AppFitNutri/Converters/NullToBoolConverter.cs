using System.Globalization;

namespace AppFitNutri.Converters;

public class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isInverse = parameter?.ToString() == "Inverse";
        var isNull = value == null;
        
        return isInverse ? isNull : !isNull;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
