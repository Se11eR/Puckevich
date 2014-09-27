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
    public enum AudioIcon
    {
        Play,
        Pause,
        Stop
    }

    public class AudioModel: INotifyPropertyChanged
    {
        private readonly IAudio __InternalAudio;
        private readonly IManagedPlayable __Playable;
        private  AudioIcon __AudioIcon;
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

        public AudioIcon AudioIcon
        {
            get
            {
                return __AudioIcon;
            }
            set
            {
                __AudioIcon = value;
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

        public void MediaButtonClicked()
        {
            switch (__Playable.State)
            {
                case PlayingState.NotInit:
                    __Playable.Play();
                    AudioIcon = AudioIcon.Pause;
                    break;
                case PlayingState.Stopped:
                    __Playable.Play();
                    AudioIcon = AudioIcon.Pause;
                    break;
                case PlayingState.Paused:
                    __Playable.Play();
                    AudioIcon = AudioIcon.Pause;
                    break;
                case PlayingState.Playing:
                    __Playable.Pause();
                    AudioIcon = AudioIcon.Play;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
