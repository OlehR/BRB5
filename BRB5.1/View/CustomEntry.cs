using System.Globalization;
using BRB5.Model;

namespace BRB6.View
{
    public class CustomEntry : Entry
    {

    }
    public class CodeUnitToKeyboardConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int codeUnit &&
                codeUnit == Config.GetCodeUnitWeight) 
            {
                return Keyboard.Telephone;
            }

            return Keyboard.Numeric;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
