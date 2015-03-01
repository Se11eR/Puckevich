using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PuckevichPlayer.Controls
{
    public class SliderWithProgress : Slider
    {
        public static readonly DependencyProperty ProgressPercentsProperty =
            DependencyProperty.Register("ProgressPercents",
                                        typeof(double),
                                        typeof(SliderWithProgress),
                                        new FrameworkPropertyMetadata(default(double), ProgressChanged));

        public static readonly DependencyProperty ProgressBackgroundProperty =
            DependencyProperty.Register("ProgressBackground",
                                        typeof(SolidColorBrush),
                                        typeof(SliderWithProgress),
                                        new FrameworkPropertyMetadata(default(SolidColorBrush),
                                                               ProgressBackgroundChanged));

       

        public SliderWithProgress()
        {
            var style = Application.Current.FindResource("CustomFlatSlider") as Style;
            Style = style;
        }

        private static void ProgressBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = d as Slider;
            if (slider == null)
                return;

            var border = slider.FindName("ProgressBorder") as Border;
            if (border == null)
                return;

            border.Background = (SolidColorBrush)e.NewValue;
        }

        private static void ProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = d as Slider;
            if (slider == null)
                return;

            var border = slider.FindName("ProgressBorder") as Border;
            if (border == null)
                return;

            border.Width = ((double)e.NewValue / 100) * (double)slider.GetValue(ActualWidthProperty);
        }

        public SolidColorBrush ProgressBackground
        {
            get
            {
                return (SolidColorBrush)GetValue(ProgressBackgroundProperty);
            }
            set
            {
                SetValue(ProgressBackgroundProperty, value);
            }
        }

        public double ProgressPercents
        {
            get
            {
                return (double)GetValue(ProgressPercentsProperty);
            }
            set
            {
                SetValue(ProgressPercentsProperty, value);
            }
        }
    }
}
