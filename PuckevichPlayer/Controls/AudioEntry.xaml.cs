using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        private void SlidInPlaybackBarAnimation_OnCompleted(object sender, EventArgs e)
        {
            PlaybackBorder.Margin = new Thickness(0, 50, 0, 0);
        }

        private void SlidOutPlaybackBarAnimation_OnCompleted(object sender, EventArgs e)
        {
            PlaybackBorder.Margin = new Thickness(0, 0, 0, 0);
        }
    }
}
