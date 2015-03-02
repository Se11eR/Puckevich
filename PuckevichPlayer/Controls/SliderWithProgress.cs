using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            Slider slider;
            Border border;
            if (ExtractProgressBorder(d, out slider, out border)) return;

            border.Background = (SolidColorBrush)e.NewValue;
        }

        private static void ProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Slider slider;
            Border border;
            if (ExtractProgressBorder(d, out slider, out border)) return;

            var valueProgress = (double)slider.GetValue(ValueProperty) / ((double)slider.GetValue(MaximumProperty) -
                                                                          (double)slider.GetValue(MinimumProperty));
            var diff = (double)e.NewValue - valueProgress * 100;
            if (diff > 0)
            {
                border.Width = (diff / 100) * (double)slider.GetValue(ActualWidthProperty);
            }
        }

        private static bool ExtractProgressBorder(DependencyObject d, out Slider slider, out Border border)
        {
            border = null;

            slider = d as Slider;
            if (slider == null)
                return true;

            var template = slider.Template.LoadContent() as FrameworkElement;
            if (template == null)
                return true;

            var button = template.FindName("ProgressContainer") as RepeatButton;
            if (button == null)
                return true;

            var template2 = button.Template.LoadContent() as FrameworkElement;
            if (template2 == null)
                return true;

            border = template2.FindName("ProgressBorder") as Border;

            if (border == null)
                return true;

            return false;
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
