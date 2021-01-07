using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TagFinder.ViewModels.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class TextToPasswordConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new String('*', value?.ToString().Length ?? 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert password to text.");
        }
    }
}
