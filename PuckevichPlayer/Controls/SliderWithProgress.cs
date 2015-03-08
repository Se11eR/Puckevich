using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
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

        public static readonly DependencyProperty PseudoValueProperty =
            DependencyProperty.Register("PseudoValue",
                                        typeof(double),
                                        typeof(SliderWithProgress),
                                        new FrameworkPropertyMetadata(default(double)));

        private readonly ProgressBar __DownloadedBar = null;
        private readonly RepeatButton __LeftButton;
        private readonly RepeatButton __RightButton;
        private readonly Thumb __Thumb;

        public SliderWithProgress()
        {
            var style = Application.Current.FindResource("CustomFlatSlider") as Style;
            Style = style;

            ExtractProgressBorder(out __DownloadedBar, out __LeftButton, out __RightButton, out __Thumb);

            __LeftButton.PreviewMouseUp += (sender, args) =>
            {
                var btn = sender as RepeatButton;
                if (btn == null)
                    return;

                var pos = args.GetPosition(btn).X;
                var btnRel = pos / btn.ActualWidth;
                var sliderRel = btn.ActualWidth / ActualWidth;

                OnChangeValueClick(btnRel * sliderRel * 100);
            };

            __RightButton.PreviewMouseUp += (sender, args) =>
            {
                var btn = sender as RepeatButton;
                if (btn == null)
                    return;

                var pos = args.GetPosition(btn).X;
                var btnRel = pos / btn.ActualWidth;
                var sliderRel = btn.ActualWidth / ActualWidth;

                var leftRel = __LeftButton.ActualWidth / ActualWidth;
                var thumbRel = __Thumb.ActualWidth / ActualWidth;

                OnChangeValueClick((leftRel + thumbRel + btnRel * sliderRel) * 100);
            };
        }

        private static void ProgressBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = d as SliderWithProgress;
            if (slider == null || slider.__DownloadedBar == null)
                return;

            slider.__DownloadedBar.Foreground = (SolidColorBrush)e.NewValue;
        }

        private static void ProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = d as SliderWithProgress;
            if (slider == null || slider.__DownloadedBar == null)
                return;

            var actualWidth = (double)slider.GetValue(ActualWidthProperty);

            if (actualWidth <= 0)
                return;

            var w = (double)e.NewValue / 100 * actualWidth;
            w -= (double)slider.__LeftButton.GetValue(ActualWidthProperty) +
                 (double)slider.__Thumb.GetValue(ActualWidthProperty);

            if (w > 0 && slider.__DownloadedBar.Value < w)
            {
                slider.__DownloadedBar.Value = 100 * w
                                               / (double)slider.__DownloadedBar.GetValue(ActualWidthProperty);
            }
        }

        private void OnChangeValueClick(double newValue)
        {
            var handler = ChangeValueClick;
            if (handler != null)
            {
                handler(this, newValue);
            }
        }

        private void ExtractProgressBorder(out ProgressBar progress,
                                           out RepeatButton left,
                                           out RepeatButton right,
                                           out Thumb thumb)
        {
            ApplyTemplate();
            right = Template.FindName("RightButton", (Slider)this) as RepeatButton;
            left = Template.FindName("LeftButton", (Slider)this) as RepeatButton;
            thumb = Template.FindName("Thumb", (Slider)this) as Thumb;

            progress = null;
            if (right == null)
                return;

            right.ApplyTemplate();
            progress = right.Template.FindName("Progress", right) as ProgressBar;
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

        public delegate void ChangeValueClickEventHandler(object sender, double newValue);

        public event ChangeValueClickEventHandler ChangeValueClick;

        public double PseudoValue
        {
            get
            {
                return (double)GetValue(PseudoValueProperty);
            }
            set
            {
                SetValue(PseudoValueProperty, value);
            }
        }
    }
}
