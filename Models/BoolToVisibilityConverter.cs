using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WinCry.Models
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            switch ((bool?)value)
            {
                case true:
                    return Visibility.Visible;
                case false:
                    return Visibility.Hidden;
                case null:
                    return Visibility.Collapsed;
                default:
                    return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
