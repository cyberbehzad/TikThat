using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TikThat.Converters
{
    public class IsGreaterThanConverter : IValueConverter
    {
        // ConverterParameter should be a number (int, double, etc.)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            double number;
            double compareTo;

            try
            {
                number = System.Convert.ToDouble(value, culture);
                compareTo = System.Convert.ToDouble(parameter, culture);
            }
            catch
            {
                return false;
            }

            return number > compareTo;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
