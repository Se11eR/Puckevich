using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PuckevichCore;
using PuckevichCore.Interfaces;

namespace PuckevichPlayer
{
    public class AudioModel: INotifyPropertyChanged
    {
        private readonly IAudio __InternalAudio;
        private readonly IManagedPlayable __Playable;
        private PlayingState __PlayingState;
        private int __TimePlayed;
        private double __Downloaded;

        public AudioModel(IAudio internalAudio)
        {
            __InternalAudio = internalAudio;
            __Playable = __InternalAudio.Playable;

            __InternalAudio.Playable.PlayingStateChanged += sender => AudioState = sender.State;
            __InternalAudio.Playable.PercentsDownloadedChanged += 
                sender => Downloaded = sender.PercentsDownloaded;
            __InternalAudio.Playable.SecondsPlayedChanged += sender => TimePlayed = sender.SecondsPlayed;

            __Downloaded = __InternalAudio.Playable.PercentsDownloaded;
        }

        public string Title
        {
            get
            {
                return __InternalAudio.Title;
            }
        }

        public string Artist
        {
            get
            {
                return __InternalAudio.Artist;
            }
        }

        public PlayingState AudioState
        {
            get
            {
                return __PlayingState;
            }
            set
            {
                __PlayingState = value;
                OnPropertyChanged();
            }
        }

        public int Duration
        {
            get
            {
                return __InternalAudio.Duration;
            }
        }

        public int TimePlayed
        {
            get
            {
                //new DateTime().AddSeconds(__TimePlayed).ToString("mm:ss");
                return __TimePlayed;
            }
            private set
            {
                __TimePlayed = value;
                OnPropertyChanged();
            }
        }

        public double Downloaded
        {
            get
            {
                return __Downloaded;
            }
            private set
            {
                __Downloaded = value;
                OnPropertyChanged();
                OnPropertyChanged("IsCached");
            }
        }

        public bool IsCached
        {
            get
            {
                return Math.Abs(__Downloaded - 100.0) < 0.01;
            }
        }

        public void AudioEntryClicked()
        {
            switch (__Playable.State)
            {
                case PlayingState.NotInit:
                    __Playable.Play();
                    break;
                case PlayingState.Stopped:
                    __Playable.Play();
                    break;
                case PlayingState.Paused:
                    __Playable.Play();
                    break;
                case PlayingState.Playing:
                    __Playable.Pause();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task StopAsync()
        {
            if (__Playable.State != PlayingState.Stopped)
            {
                await __Playable.StopAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
