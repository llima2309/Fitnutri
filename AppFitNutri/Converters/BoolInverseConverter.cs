using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFitNutri.Converters
{
    public class BoolInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : value;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : value;
    }
}
