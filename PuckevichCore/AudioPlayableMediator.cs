using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PuckevichCore
{
    internal class AudioPlayableMediator : IManagedPlayable
    {
        private readonly AudioPlayable __InternalAudioPlayable;

        internal AudioPlayableMediator(IAudioStorage storage, IWebDownloader downloader, IAudio audio, Uri url)
        {
            __InternalAudioPlayable = new AudioPlayable(audio, storage, downloader, url);
        }

        private void OnPlayingStateChanged()
        {
            var handler = PlayingStateChanged;
            if (handler != null)
                handler(this);
        }

        private void CheckInit()
        {
            if (__InternalAudioPlayable.State == PlayingState.NotInit)
            {
                throw new ApplicationException("AudioPlayableMediator was not initialized!");
            }
        }

        public void Play()
        {
            CheckInit();

            if (__InternalAudioPlayable.State != PlayingState.Playing)
            {
                __InternalAudioPlayable.Play();
                OnPlayingStateChanged();
            }
        }

        public void Pause()
        {
            CheckInit();

            if (__InternalAudioPlayable.State != PlayingState.Paused && __InternalAudioPlayable.State != PlayingState.NotInit)
            {
                __InternalAudioPlayable.Pause();
                OnPlayingStateChanged();
            }
        }

        public void Stop()
        {
            CheckInit();

            if (__InternalAudioPlayable.State != PlayingState.Stopped && __InternalAudioPlayable.State != PlayingState.NotInit)
            {
                __InternalAudioPlayable.Stop();
                OnPlayingStateChanged();
            }
        }

        public async Task StopAsync()
        {
            CheckInit();

            if (__InternalAudioPlayable.State != PlayingState.Stopped && __InternalAudioPlayable.State != PlayingState.NotInit)
            {
                await __InternalAudioPlayable.StopAsync();
                OnPlayingStateChanged();
            }
        }

        public double Downloaded
        {
            get
            {
                return __InternalAudioPlayable.Downloaded;
            }
        }

        public event PlayingStateChangedEventHandler PlayingStateChanged;

        public int SecondsPlayed
        {
            get
            {
                return __InternalAudioPlayable.SecondsPlayed;
            }
        }

        public PlayingState State
        {
            get { return __InternalAudioPlayable.State; }
        }

        public void Init()
        {
            __InternalAudioPlayable.Init();
        }

        public async Task InitAsync()
        {
            await __InternalAudioPlayable.InitAsync();
        }
    }
}
