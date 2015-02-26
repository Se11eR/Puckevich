using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PuckevichPlayer.Converters
{
    public class DownloadedPercentsToRatioWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.Length != 3)
                    return 0.0;

                if (!(values[0] is double && values[1] is double && values[2] is Thickness))
                    return 0.0;

                var downloadedPercents = (double)values[0];
                var controlWidth = (double)values[1];
                var offset = Math.Abs(((Thickness)values[2]).Left);

                return ((downloadedPercents / 100) * controlWidth) + offset;
            }
            catch (Exception)
            {
                return 0.0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
