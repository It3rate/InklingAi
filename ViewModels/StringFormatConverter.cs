namespace InklingAi.ViewModels
{
    using Avalonia.Data.Converters;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class StringFormatConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            string result = "";
            if (values.Count > 1 && values[0] is double d && values[1] is string formatString)
            {
                result = d.ToString(formatString, culture);
            }
            return result;
        }
    }
}
