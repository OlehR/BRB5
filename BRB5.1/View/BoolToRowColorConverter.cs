using System.Globalization;

namespace BRB6.View
{
    public class BoolToRowColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? Color.FromArgb("#F0F0F0") : Colors.White;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
