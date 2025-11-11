using System.Globalization;

namespace AppFitNutri.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int status)
        {
            return status switch
            {
                0 => Color.FromArgb("#FF9800"), // Pendente - Laranja
                1 => Color.FromArgb("#4CAF50"), // Confirmado - Verde
                2 => Color.FromArgb("#F44336"), // Cancelado - Vermelho
                _ => Color.FromArgb("#9E9E9E")  // Outro - Cinza
            };
        }
        return Color.FromArgb("#9E9E9E");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

