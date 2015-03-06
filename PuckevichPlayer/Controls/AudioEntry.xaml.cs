using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PuckevichCore;

namespace PuckevichPlayer.Controls
{
    /// <summary>
    /// Interaction logic for AudioEntry.xaml
    /// </summary>
    public partial class AudioEntry : UserControl, INotifyPropertyChanged
    {
        private bool __IsDraggingNow = false;

        public AudioEntry()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PlaybackSlider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            ((AudioModel)DataContext).TimePlayed = PlaybackSlider.Value / 100
                                                   * ((AudioModel)DataContext).Duration;
            __IsDraggingNow = false;
        }

        private void PlaybackSlider_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            __IsDraggingNow = true;
        }

        private void PlaybackSlider_OnTargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.Property.Name == "PseudoValue")
                if (!__IsDraggingNow)
                    PlaybackSlider.Value = PlaybackSlider.PseudoValue;
        }
    }
}
