using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PuckevichPlayer.Converters
{
    public class DownloadedPercentsToRatioWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.Length != 2)
                    return 0.0;

                if (!(values[0] is double && values[1] is double))
                    return 0.0;

                var downloadedPercents = (double)values[0];
                var controlWidth = (double)values[1];

                return (downloadedPercents / 100) * controlWidth;
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
