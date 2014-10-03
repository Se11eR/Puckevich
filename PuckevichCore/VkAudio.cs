using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
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

            __InternalPlayable = new PlayableAudio(this, storage, downloader, audioId, url);
            __InternalPlayable.AudioStopped += playable => OnAudioStopped();
        }

        private void OnAudioStopped()
        {
            var handler = AudioStopped;
            if (handler != null)
                handler(Playable);
        }

        private async Task CheckInit()
        {
            if (!__IsInitialized || __InternalPlayable.State == PlayingState.Stopped)
            {
                await __InternalPlayable.Init();
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
            }
        }

        

        public void Pause()
        {
            CheckInit();

            if (__InternalPlayable.State == PlayingState.Playing)
            {
                __InternalPlayable.Pause();
            }
        }

        public async Task Stop()
        {
            await CheckInit();

            if (__InternalPlayable.State == PlayingState.Playing)
            {
                await __InternalPlayable.Stop();
            }
        }

        public double PercentageDownloaded
        {
            get
            {
                return __InternalPlayable.PercentageDownloaded;
            }
        }

        public event AudioStoppedEvent AudioStopped;

        public event AudioStalledEvent AudioStalled;

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

        public async Task Init()
        {
            __IsInitialized = true;
            await __InternalPlayable.Init();
        }
    }
}
