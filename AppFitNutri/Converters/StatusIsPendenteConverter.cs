using System.Globalization;

namespace AppFitNutri.Converters;

public class StatusIsPendenteConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int status)
        {
            return status == 0; // Pendente
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

