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
    public class VkAudio : IAudio
    {
        private readonly long __AudioId;
        private readonly long __UserId;
        private readonly string __Title;
        private readonly string __Artist;
        private readonly int __Duration;

        private PlayableAudio __Playable;
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

            __Playable = new PlayableAudio(storage, downloader, audioId, url);
            __Playable.AudioStopped += playable => OnAudioStopped();
        }

        private void OnAudioStopped()
        {
            var handler = AudioStopped;
            if (handler != null)
                handler(this);
        }

        private void CheckInit()
        {
            if (!__IsInitialized || __Playable.State == PlayingState.Stopped)
            {
                __Playable.Init();
                __IsInitialized = true;
            }
        }

        public long SongId
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

            lock (__PlayableLock)
            {
                if (__Playable.State != PlayingState.Playing)
                {
                    __Playable.Play();
                }
            }
        }

        

        public void Pause()
        {
            CheckInit();

            lock (__PlayableLock)
            {
                if (__Playable.State == PlayingState.Playing)
                {
                    __Playable.Pause();
                }
            }
        }

        public void Stop()
        {
            CheckInit();

            lock (__PlayableLock)
            {
                if (__Playable.State == PlayingState.Playing)
                {
                    __Playable.Stop();
                }
            }
        }

        public double PercentagePlayed
        {
            get
            {
                return __Playable.PercentagePlayed;
            }
        }

        public double PercentageDownloaded
        {
            get
            {
                return __Playable.PercentageDownloaded;
            }
        }

        public event AudioStoppedEvent AudioStopped;

        public event AudioStalledEvent AudioStalled;

        public int SecondsPlayed
        {
            get
            {
                return __Playable.SecondsPlayed;
            }
        }


        public PlayingState State
        {
            get { return __Playable.State; }
        }
    }
}
