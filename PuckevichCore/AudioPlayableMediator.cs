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
            if (__InternalPlayable.State == PlayingState.NotInit)
            {
                throw new ApplicationException("AudioPlayableMediator was not initialized!");
            }
        }

        public void Play()
        {
            CheckInit();

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
                OnPlayingStateChanged();
                __PlaybackTimer.Stop();
                OnSecondsPlayedChanged();
            }
        }

        public async Task StopAsync()
        {
            CheckInit();

            if (__InternalPlayable.State != PlayingState.Stopped && __InternalPlayable.State != PlayingState.NotInit)
            {
                await __InternalPlayable.StopAsync();
                OnPlayingStateChanged();
                __PlaybackTimer.Stop();
                OnSecondsPlayedChanged();
            }
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

        public void Init()
        {
            __InternalPlayable.Init();
        }

        public async Task InitAsync()
        {
            await __InternalPlayable.InitAsync();
        }
    }
}
