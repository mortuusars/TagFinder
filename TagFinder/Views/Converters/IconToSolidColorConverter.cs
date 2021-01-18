using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TagFinder.Resources;

namespace TagFinder.Views.Converters
{
    [ValueConversion(typeof(Icons), typeof(SolidColorBrush))]
    public class IconToSolidColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((Icons)value)
            {
                case Icons.Info:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#70abdc"));
                case Icons.Error:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ca5252"));
                default:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a4c1c4"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
