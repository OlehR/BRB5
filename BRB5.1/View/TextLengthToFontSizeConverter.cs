using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace BRB6.View
{
    public class TextLengthToFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrEmpty(text)) return 22;

            int length = text.Length;

            // Ваша логіка діапазонів:
            if (length <= 40) return 22; // Стандартний
            if (length <= 50) return 19;
            if (length <= 70) return 16;
            if (length <= 90) return 14;
            return 12; // Для > 90 символів
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
