using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuckevichCore.Interfaces;
using VkNet.Model.Attachments;

namespace PuckevichCore
{
    internal class AudioInfo : IAudio
    {
        internal class AudioInfoFactory
        {
            private readonly IAudioStorage __Storage;
            private readonly IWebDownloader __Downloader;

            public AudioInfoFactory(IAudioStorage storage, IWebDownloader downloader)
            {
                __Storage = storage;
                __Downloader = downloader;
            }

            public AudioInfo Create(long userId,
                int index,
                Audio audio,
                Uri url,
                PlayingStateChangedEventHandler handler = null)
            {
                var info = new AudioInfo(__Storage,
                    __Downloader,
                    audio.Id,
                    userId,
                    audio.Title,
                    audio.Artist,
                    audio.Duration,
                    index,
                    url);

                if (handler != null)
                {
                    info.Playable.PlayingStateChanged += handler;
                }
                return info;
            }

            public AudioInfo Create(long userId,
                int index,
                IAudio audio,
                Uri url,
                PlayingStateChangedEventHandler handler = null)
            {
                var info = new AudioInfo(__Storage,
                    __Downloader,
                    audio.AudioId,
                    userId,
                    audio.Title,
                    audio.Artist,
                    audio.Duration,
                    index,
                    url);

                if (handler != null)
                {
                    info.Playable.PlayingStateChanged += handler;
                }
                return info;
            }
        }

        private readonly long __AudioId;
        private readonly long __UserId;
        private readonly string __Title;
        private readonly string __Artist;
        private readonly int __Duration;
        private readonly IManagedPlayable __Playable;
        private readonly int __Index;

        private AudioInfo(IAudioStorage storage, IWebDownloader downloader,
            long audioId, long userId, string title, string artist, int duration, int index, Uri url)
        {
            __AudioId = audioId;
            __UserId = userId;
            __Title = title;
            __Artist = artist;
            __Duration = duration;
            __Index = index;

            __Playable = new AudioPlayableMediator(storage, downloader, this, url);
        }

        public long AudioId
        {
            get
            {
                return __AudioId;
            }
        }

        public long UserId
        {
            get
            {
                return __UserId;
            }
        }

        public string Title
        {
            get
            {
                return __Title;
            }
        }

        public string Artist
        {
            get
            {
                return __Artist;
            }
        }

        public int Duration
        {
            get
            {
                return __Duration;
            }
        }

        public int Index
        {
            get { return __Index; }
        }

        public IManagedPlayable Playable
        {
            get
            {
                return __Playable;
            }
        }
    }
}
