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

        private readonly Border __ProgressBorder = null;

        public SliderWithProgress()
        {
            var style = Application.Current.FindResource("CustomFlatSlider") as Style;
            Style = style;

            __ProgressBorder = ExtractProgressBorder();
        }

        private static void ProgressBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = d as SliderWithProgress;
            if (slider == null || slider.__ProgressBorder == null)
                return;

            slider.__ProgressBorder.Background = Brushes.Blue;
        }

        private static void ProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = d as SliderWithProgress;
            if (slider == null || slider.__ProgressBorder == null)
                return;

            var valueProgress = (double)slider.GetValue(ValueProperty) / ((double)slider.GetValue(MaximumProperty) -
                                                                          (double)slider.GetValue(MinimumProperty));
            var diff = (double)e.NewValue - valueProgress * 100;
            if (diff > 0)
            {
                slider.__ProgressBorder.SetValue(Border.WidthProperty,
                                                 (diff / 100) * (double)slider.GetValue(ActualWidthProperty));
            }
        }

        private Border ExtractProgressBorder()
        {
            var template = Template.LoadContent() as FrameworkElement;
            if (template == null)
                return null;

            var button = template.FindName("ProgressContainer") as RepeatButton;
            if (button == null)
                return null;

            var template2 = button.Template.LoadContent() as FrameworkElement;
            if (template2 == null)
                return null;

            var border = template2.FindName("ProgressBorder") as Border;

            if (border == null)
                return null;

            return border;
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
