using System;
using System.Threading.Tasks;
using System.Timers;
using PuckevichCore.Interfaces;

namespace PuckevichCore
{
    internal class AudioPlayableMediator : IManagedPlayable
    {
        private readonly AudioPlayable __InternalPlayable;
        private readonly Timer __PlaybackTimer = new Timer(1000);
        private PlayingState __State = PlayingState.Stopped;

        public event PlayingStateChangedEventHandler PlayingStateChanged;
        public event PercentsDownloadedChangedEventHandler PercentsDownloadedChanged;
        public event SecondsPlayedChangedEventHandler SecondsPlayedChanged;

        internal AudioPlayableMediator(IAudioStorage storage, IWebDownloader downloader, IAudio audio, Uri url)
        {
            __InternalPlayable = new AudioPlayable(audio, storage, downloader, url);
            __InternalPlayable.DownloadedFracionChanged += OnPercentsDownloadedChanged;
            __InternalPlayable.AudioNaturallyEnded += WhenStop;

            __PlaybackTimer.Elapsed += (sender, args) =>
            {
                OnSecondsPlayedChanged();
                if (__InternalPlayable.SecondsPlayed >= audio.Duration)
                    Stop();
            };
            __PlaybackTimer.AutoReset = true;
        }

        private void OnPlayingStateChanged()
        {
            var handler = PlayingStateChanged;
            if (handler != null)
                handler(this);
        }

        private void OnPercentsDownloadedChanged()
        {
            var handler = PercentsDownloadedChanged;
            if (handler != null)
                handler(this);
        }

        private void OnSecondsPlayedChanged()
        {
            var handler = SecondsPlayedChanged;
            if (handler != null)
                handler(this);
        }

        private void CheckInit()
        {
            if (!__InternalPlayable.TasksInitialized)
            {
                __InternalPlayable.Init();
            }
        }

        public void Play()
        {
            if (__State == PlayingState.Playing)
                return;

            CheckInit();
            WhenPlay();
        }

        private void WhenPlay()
        {
            __InternalPlayable.Play();
            __State = PlayingState.Playing;
            OnPlayingStateChanged();
            __PlaybackTimer.Start();
        }

        public void Pause()
        {
            if (__State == PlayingState.Paused)
                return;

            CheckInit();
            WhenPause();
        }

        private void WhenPause()
        {
            __InternalPlayable.Pause();
            __State = PlayingState.Paused;
            OnPlayingStateChanged();
            __PlaybackTimer.Stop();
        }

        public void Stop()
        {
            if (__State == PlayingState.Stopped)
                return;

            __InternalPlayable.Stop();
            WhenStop();
        }

        private void WhenStop()
        {
            __PlaybackTimer.Stop();
            __State = PlayingState.Stopped;
            OnPlayingStateChanged();
            OnSecondsPlayedChanged();
        }

        public double PercentsDownloaded
        {
            get
            {
                return __InternalPlayable.DownloadedFracion * 100;
            }
        }

        
        public int SecondsPlayed
        {
            get
            {
                return __InternalPlayable.SecondsPlayed;
            }
        }

        public PlayingState State
        {
            get { return __State; }
        }
    }
}
