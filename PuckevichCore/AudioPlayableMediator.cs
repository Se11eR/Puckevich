using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PuckevichCore
{
    internal class AudioPlayableMediator : IManagedPlayable
    {
        private readonly AudioPlayable __InternalAudioPlayable;
        private bool __IsInitialized;

        internal AudioPlayableMediator(IAudioStorage storage, IWebDownloader downloader, IAudio audio, Uri url)
        {
            __InternalAudioPlayable = new AudioPlayable(audio, storage, downloader, url);
            __InternalAudioPlayable.PlayingStateChanged += playable => OnPlayingStateChanged();
        }

        private void OnPlayingStateChanged()
        {
            var handler = PlayingStateChanged;
            if (handler != null)
                handler(AudioPlayable);
        }

        private void CheckInit()
        {
            if (!__IsInitialized || __InternalAudioPlayable.State == PlayingState.Stopped)
            {
                throw new ApplicationException("AudioPlayableMediator was not initialized!");
            }
        }

        private async Task CheckAndInitAsync()
        {
            if (!__IsInitialized || __InternalAudioPlayable.State == PlayingState.Stopped)
            {
                await __InternalAudioPlayable.InitAsync();
                __IsInitialized = true;
            }
        }

        public void Play()
        {
            CheckInit();

            if (__InternalAudioPlayable.State != PlayingState.Playing)
            {
                __InternalAudioPlayable.Play();
                AudioManager.Instance.OpenedChannels.Add(__InternalAudioPlayable);
            }
        }

        public void Pause()
        {
            CheckInit();

            if (__InternalAudioPlayable.State != PlayingState.Paused && __InternalAudioPlayable.State != PlayingState.NotInit)
            {
                __InternalAudioPlayable.Pause();
                AudioManager.Instance.OpenedChannels.Add(__InternalAudioPlayable);
            }
        }

        public void Stop()
        {
            CheckInit();

            if (__InternalAudioPlayable.State != PlayingState.Stopped && __InternalAudioPlayable.State != PlayingState.NotInit)
            {
                __InternalAudioPlayable.Stop();
                AudioManager.Instance.OpenedChannels.Remove(__InternalAudioPlayable);
            }
        }

        public async Task StopAsync()
        {
            await CheckAndInitAsync();

            if (__InternalAudioPlayable.State != PlayingState.Stopped && __InternalAudioPlayable.State != PlayingState.NotInit)
            {
                await __InternalAudioPlayable.StopAsync();
                AudioManager.Instance.OpenedChannels.Remove(__InternalAudioPlayable);
            }
        }

        public double Downloaded
        {
            get
            {
                return __InternalAudioPlayable.Downloaded;
            }
        }

        public event PlayingStateChangedEvent PlayingStateChanged;

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

        public IManagedPlayable AudioPlayable
        {
            get { return __InternalAudioPlayable; }
        }

        public void Init()
        {
            __IsInitialized = true;
            __InternalAudioPlayable.Init();
        }

        public async Task InitAsync()
        {
            await __InternalAudioPlayable.InitAsync();
            __IsInitialized = true;
        }
    }
}
