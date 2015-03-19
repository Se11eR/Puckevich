using System;
using PuckevichCore.CacheStorage;
using PuckevichCore.Interfaces;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace PuckevichCore
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
}
