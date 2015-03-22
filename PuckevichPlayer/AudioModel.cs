using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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

        public AudioModel(IAudio internalAudio)
        {
            __InternalAudio = internalAudio;
            __Playable = __InternalAudio.Playable;

            __InternalAudio.Playable.PlayingStateChanged += sender => AudioState = sender.State;
            __InternalAudio.Playable.PercentsDownloadedChanged +=
                sender =>
                {
                    OnPropertyChanged("IsCached");
                    OnPropertyChanged("Downloaded");
                };
            __InternalAudio.Playable.SecondsPlayedChanged += sender => OnPropertyChanged("TimePlayed");

            Downloaded = __InternalAudio.Playable.PercentsDownloaded;
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
            set
            {
            }
        }

        public double TimePlayed
        {
            get
            {
                //new DateTime().AddSeconds(__TimePlayed).ToString("mm:ss");
                return __Playable.SecondsPlayed;
            }
            set
            {
                __Playable.SecondsPlayed = value;
            }
        }

        public double Downloaded
        {
            get
            {
                return __Playable.PercentsDownloaded;
            }
            private set
            {
            }
        }

        public bool IsCached
        {
            get
            {
                return Downloaded == 100.0;
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

        public void Stop()
        {
            if (__Playable.State != PlayingState.Stopped)
            {
                 __Playable.Stop();
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
