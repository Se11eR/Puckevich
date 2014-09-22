using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PuckevichPlayer.Conerters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class AudioNameToEllipsisConerter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var maxLength = 30;
            if (parameter is int)
                maxLength = (int)parameter;

            var name = (string)value;
            if (name.Length > maxLength)
                name = name.Substring(0, maxLength - 3) + "...";

            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
