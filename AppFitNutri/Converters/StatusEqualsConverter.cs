using System.Globalization;

namespace AppFitNutri.Converters;

/// <summary>
/// Converter que verifica se um valor é igual ao parâmetro fornecido
/// </summary>
public class StatusEqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        var valueStr = value.ToString();
        var paramStr = parameter.ToString();

        return string.Equals(valueStr, paramStr, StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

