using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TagFinder.ViewModels.Converters
{
    [ValueConversion(typeof(int), typeof(SolidColorBrush))]
    public class CountForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush((Color) ColorConverter.ConvertFromString("#ff6666"));
            else if ((int)value > 30)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff6666"));
            else
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
