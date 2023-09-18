using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace InklingAi.ViewModels
{
    internal class PathToBitmapConverter : IValueConverter
    {
        public static PathToBitmapConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Bitmap? result = null;
            if (value != null && value is string rawUri && targetType.IsAssignableFrom(typeof(Bitmap)))
            {
                result = new Bitmap(rawUri);
            }
            return result;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
