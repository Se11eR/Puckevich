using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using PuckevichPlayer.Controls;
using Page = System.Windows.Controls.Page;

namespace PuckevichPlayer
{
    /// <summary>
    /// Interaction logic for p_Player.xaml
    /// </summary>
    public partial class p_Player : Page, INotifyPropertyChanged
    {
        private IList<AudioModel> __List;
        private AudioModel __CurrentActive;

        public p_Player(IList<AudioModel> list)
        {
            InitializeComponent();
            DataContext = this;
            AudioList = list;
        }

        public IList<AudioModel> AudioList
        {
            get
            {
                return __List;
            }
            set
            {
                __List = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void AudioEntry_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is AudioEntry))
                return;

            var audioModel = (sender as AudioEntry).DataContext as AudioModel;
            if (audioModel == null)
                return;

            await audioModel.AudioEntryClicked();
            if (__CurrentActive == null)
            {
                __CurrentActive = audioModel;
                return;
            }

            if (__CurrentActive != audioModel)
            {
                switch (audioModel.AudioState)
                {
                    case PlayingState.NotInit:
                        break;
                    case PlayingState.Stopped:
                        break;
                    case PlayingState.Paused:
                        await __CurrentActive.StopAsync();
                        __CurrentActive = audioModel;
                        break;
                    case PlayingState.Playing:
                        await __CurrentActive.StopAsync();
                        __CurrentActive = audioModel;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                switch (audioModel.AudioState)
                {
                    case PlayingState.NotInit:
                        __CurrentActive = null;
                        break;
                    case PlayingState.Stopped:
                        __CurrentActive = null;
                        break;
                }
            }
        }
    }
}
