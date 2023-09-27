using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace InklingAi.ViewModels
{

    public class DoubleStringConverter : IValueConverter
    {
        public static DoubleStringConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string result = "";
            if (value is double d)
            {
                if (parameter is string format && format != "")
                {
                    result = d.ToString(format, culture);
                }
                else
                {
                    result = d.ToString("0", culture);
                }
            }
            return result;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s && double.TryParse(s, NumberStyles.Any, culture, out double result))
            {
                return result;
            }
            return 0d;  // Or another suitable default
        }
    }
}