using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuckevichCore.Interfaces;

namespace PuckevichCore
{
    internal class AudioInfo : IAudio
    {
        private long __AudioId;
        private long __UserId;
        private string __Title;
        private string __Artist;
        private int __Duration;
        private IManagedPlayable __Playable;
        private int __Index;

        internal AudioInfo(IAudioStorage storage, IWebDownloader downloader,
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
