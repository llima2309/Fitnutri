using System.Globalization;

namespace AppFitNutri.Converters;

public class StatusToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int status)
        {
            return status switch
            {
                0 => "Pendente",
                1 => "Confirmado",
                2 => "Cancelado",
                _ => "Desconhecido"
            };
        }
        return "Desconhecido";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

