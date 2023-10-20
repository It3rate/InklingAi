namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Avalonia.Data.Converters;
    using Avalonia.Media;

    public class ColorConverter : IValueConverter
    {
        public static ColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string result = "0XFF0000";
            if (value is uint d)
            {
                result = "#" + d.ToString("X6", CultureInfo.InvariantCulture);
            }
            return new SolidColorBrush(Color.Parse(result));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            uint result = 0;
            if (value is SolidColorBrush brush)
            {
                result = (uint)((brush.Color.R << 16) | (brush.Color.G << 8) | (brush.Color.B)); ;
            }
            return result;  
        }
    }
}
