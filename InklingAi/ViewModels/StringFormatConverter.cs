namespace InklingAi.ViewModels;

using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

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

public class PercentageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        object? result;
        switch (value)
        {
            case double doubleValue:
                result = doubleValue * 100;
                break;
            case float floatValue:
                result = floatValue * 100;
                break;
            default:
                result = value;
                break;
        }
        return result;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        object? result;

        double val = 0;
        if (value is double || value is float || value is int || value is long)
        {
            val = System.Convert.ToDouble(value);
        }

        if (targetType == typeof(double))
            result = val / 100;
        else //if (targetType == typeof(float))
            result = (float)(val / 100);

        return result;
    }
}
