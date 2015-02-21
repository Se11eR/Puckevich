using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;


namespace PuckevichCore
{
    internal class AudioPlayableMediator : IManagedPlayable
    {
        private readonly AudioPlayable __InternalPlayable;
        private readonly Timer __PlaybackTimer = new Timer(1000);

        public event PlayingStateChangedEventHandler PlayingStateChanged;
        public event PercentsDownloadedChangedEventHandler PercentsDownloadedChanged;
        public event SecondsPlayedChangedEventHandler SecondsPlayedChanged;

        internal AudioPlayableMediator(IAudioStorage storage, IWebDownloader downloader, IAudio audio, Uri url)
        {
            __InternalPlayable = new AudioPlayable(audio, storage, downloader, url);
            __InternalPlayable.DownloadedFracionChanged += OnPercentsDownloadedChanged;

            __PlaybackTimer.Elapsed += (sender, args) => OnSecondsPlayedChanged();
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
            if (__InternalPlayable.State == PlayingState.NotInit || __InternalPlayable.State == PlayingState.Stopped)
            {
                __InternalPlayable.Init();
            }
        }

        private async Task CheckInitAsync()
        {
            if (__InternalPlayable.State == PlayingState.NotInit || __InternalPlayable.State == PlayingState.Stopped)
            {
                await __InternalPlayable.InitAsync();
            }
        }

        public void Play()
        {
            CheckInit();

            WhenPlay();
        }

        public async Task PlayAsync()
        {
            await CheckInitAsync();

            WhenPlay();
        }

        private void WhenPlay()
        {
            if (__InternalPlayable.State != PlayingState.Playing)
            {
                __InternalPlayable.Play();
                OnPlayingStateChanged();
                __PlaybackTimer.Start();
            }
        }

        public void Pause()
        {
            CheckInit();

            WhenPause();
        }

        public async Task PauseAsync()
        {
            await CheckInitAsync();

            WhenPause();
        }

        private void WhenPause()
        {
            if (__InternalPlayable.State != PlayingState.Paused && __InternalPlayable.State != PlayingState.NotInit)
            {
                __InternalPlayable.Pause();
                OnPlayingStateChanged();
                __PlaybackTimer.Stop();
            }
        }

        public void Stop()
        {
            CheckInit();

            if (__InternalPlayable.State != PlayingState.Stopped && __InternalPlayable.State != PlayingState.NotInit)
            {
                __InternalPlayable.Stop();
                WhenStop();
            }
        }

        public async Task StopAsync()
        {
            await CheckInitAsync();

            if (__InternalPlayable.State != PlayingState.Stopped && __InternalPlayable.State != PlayingState.NotInit)
            {
                await __InternalPlayable.StopAsync();
                WhenStop();
            }
        }

        private void WhenStop()
        {
            OnPlayingStateChanged();
            __PlaybackTimer.Stop();
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
            get { return __InternalPlayable.State; }
        }
    }
}
