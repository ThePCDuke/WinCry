using System;
using System.Globalization;
using System.Windows.Data;

namespace WinCry.Tweaks
{
    class IntToTweaksOptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (TweaksOption)value;
        }
    }
}
