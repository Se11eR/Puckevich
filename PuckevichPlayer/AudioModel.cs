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
        private double __Downloaded;
        private volatile int __IsTaskProgress = 0;

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

        private async Task CheckTaskProgress()
        {
            if (__IsTaskProgress > 0)
            {
                await Task.Run(() =>
                {
                    while (__IsTaskProgress > 0)
                        Thread.Sleep(1);
                });
            }
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

        public bool IsTaskProgress
        {
            get
            {
                return __IsTaskProgress > 0;
            }
        }

        public async Task AudioEntryClickedAsync()
        {
            await CheckTaskProgress();

            Interlocked.Exchange(ref __IsTaskProgress, 1);
            switch (__Playable.State)
            {
                case PlayingState.NotInit:
                    await __Playable.PlayAsync();
                    break;
                case PlayingState.Stopped:
                    await __Playable.PlayAsync();
                    break;
                case PlayingState.Paused:
                    await __Playable.PlayAsync();
                    break;
                case PlayingState.Playing:
                    await __Playable.PauseAsync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Interlocked.Exchange(ref __IsTaskProgress, 0);
        }

        public async Task StopAsync()
        {
            await CheckTaskProgress();

            Interlocked.Exchange(ref __IsTaskProgress, 1);
            if (__Playable.State != PlayingState.Stopped)
            {
                await __Playable.StopAsync();
            }
            Interlocked.Exchange(ref __IsTaskProgress, 0);
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
