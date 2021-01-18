using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TagFinder.Views.Converters
{
    [ValueConversion(typeof(int), typeof(Visibility))]
    public class LengthToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value == 0)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert visibility to length.");
        }
    }
}
