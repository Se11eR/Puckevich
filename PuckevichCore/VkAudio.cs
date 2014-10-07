using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PuckevichCore
{
    public class VkAudio : IAudio, IManagedPlayable
    {
        private readonly long __AudioId;
        private readonly long __UserId;
        private readonly string __Title;
        private readonly string __Artist;
        private readonly int __Duration;

        private readonly PlayableAudio __InternalPlayable;
        private bool __IsInitialized;
        private readonly object __PlayableLock = new object();

        internal VkAudio(IAudioStorage storage, IWebDownloader downloader, 
            long audioId, long userId, string title, string artist, int duration, Uri url)
        {
            __AudioId = audioId;
            __UserId = userId;
            __Title = title;
            __Artist = artist;
            __Duration = duration;

            __InternalPlayable = new PlayableAudio(this, storage, downloader, url);
            __InternalPlayable.PlayingStateChanged += playable => OnPlayingStateChanged();
        }

        private void OnPlayingStateChanged()
        {
            var handler = PlayingStateChanged;
            if (handler != null)
                handler(Playable);
        }

        private void CheckInit()
        {
            if (!__IsInitialized || __InternalPlayable.State == PlayingState.Stopped)
            {
                throw new ApplicationException("VkAudio was not initialized!");
            }
        }

        private async Task CheckAndInitAsync()
        {
            if (!__IsInitialized || __InternalPlayable.State == PlayingState.Stopped)
            {
                await __InternalPlayable.InitAsync();
                __IsInitialized = true;
            }
        }

        public long AudioId
        {
            get { return __AudioId; }
        }

        public long UserId
        {
            get { return __UserId; }
        }

        public string Title
        {
            get { return __Title; }
        }

        public string Artist
        {
            get { return __Artist; }
        }

        public int Duration
        {
            get { return __Duration; }
        }

        public void Play()
        {
            CheckInit();

            if (__InternalPlayable.State != PlayingState.Playing)
            {
                __InternalPlayable.Play();
                VkAudioManager.Instance.OpenedChannels.Add(__InternalPlayable);
            }
        }

        public void Pause()
        {
            CheckInit();

            if (__InternalPlayable.State != PlayingState.Paused && __InternalPlayable.State != PlayingState.NotInit)
            {
                __InternalPlayable.Pause();
                VkAudioManager.Instance.OpenedChannels.Add(__InternalPlayable);
            }
        }

        public void Stop()
        {
            CheckInit();

            if (__InternalPlayable.State != PlayingState.Stopped && __InternalPlayable.State != PlayingState.NotInit)
            {
                __InternalPlayable.Stop();
                VkAudioManager.Instance.OpenedChannels.Remove(__InternalPlayable);
            }
        }

        public async Task StopAsync()
        {
            await CheckAndInitAsync();

            if (__InternalPlayable.State != PlayingState.Stopped && __InternalPlayable.State != PlayingState.NotInit)
            {
                await __InternalPlayable.StopAsync();
                VkAudioManager.Instance.OpenedChannels.Remove(__InternalPlayable);
            }
        }

        public double Downloaded
        {
            get
            {
                return __InternalPlayable.Downloaded;
            }
        }

        public event PlayingStateChangedEvent PlayingStateChanged;

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

        public IManagedPlayable Playable
        {
            get { return this; }
        }

        public void Init()
        {
            __IsInitialized = true;
            __InternalPlayable.Init();
        }

        public async Task InitAsync()
        {
            await __InternalPlayable.InitAsync();
            __IsInitialized = true;
        }
    }
}
