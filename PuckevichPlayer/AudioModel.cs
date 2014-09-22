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
        private  AudioIcon __AudioIcon;
        private int __TimePlayed;
        private double __Downloaded;

        public AudioModel(IAudio internalAudio)
        {
            __InternalAudio = internalAudio;
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
                return new DateTime().AddSeconds(__InternalAudio.SecondsPlayed).ToString("mm:ss");
            }
        }

        public double Downloaded
        {
            get
            {
                return __InternalAudio.PercentageDownloaded;
            }
        }

        public void MediaButtonClicked()
        {
            switch (__InternalAudio.State)
            {
                case PlayingState.NotInit:
                    __InternalAudio.Play();
                    AudioIcon = AudioIcon.Pause;
                    break;
                case PlayingState.Stopped:
                     __InternalAudio.Play();
                    AudioIcon = AudioIcon.Pause;
                    break;
                case PlayingState.Paused:
                    __InternalAudio.Play();
                    AudioIcon = AudioIcon.Pause;
                    break;
                case PlayingState.Playing:
                    __InternalAudio.Pause();
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
