using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PuckevichPlayer.Converters
{
    public class TimePlayedAndDurationToRatioWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.Length != 3)
                    return null;

                var timePlayed = (int)values[0];
                var duration = (int)values[1];
                var controlWidth = (double)values[2];

                return ((double)timePlayed / duration) * controlWidth;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
