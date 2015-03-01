using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PuckevichPlayer.Converters
{
    public class PlaybackProgressConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.Length != 2)
                    return 0.0;

                if (!(values[0] is int && values[1] is int))
                    return 0.0;

                var timePlayed = (int)values[0];
                var duration = (int)values[1];

                return (((double)timePlayed) / duration) * 100;
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
