using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PuckevichCore;

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

        public string TimePlayed
        {
            get
            {
                return new DateTime().AddSeconds(__Playable.SecondsPlayed).ToString("mm:ss");
            }
        }

        public double Downloaded
        {
            get
            {
                return __Playable.PercentageDownloaded;
            }
        }

        public void AudioEntryClicked()
        {
            if (__Playable.State == PlayingState.NotInit || __Playable.State == PlayingState.Stopped)
            {
                __Playable.Init();
            }

            switch (__Playable.State)
            {
                case PuckevichCore.PlayingState.NotInit:
                    __Playable.Play();
                    break;
                case PuckevichCore.PlayingState.Stopped:
                    __Playable.Play();
                    break;
                case PuckevichCore.PlayingState.Paused:
                    __Playable.Play();
                    break;
                case PuckevichCore.PlayingState.Playing:
                    __Playable.Pause();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            AudioState = __Playable.State;
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
