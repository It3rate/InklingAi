using System;
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
            if (value is double d)
            {
                return d.ToString("0.#", culture);
            }
            return string.Empty;
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