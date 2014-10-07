using System;

namespace PuckevichCore
{
    internal class VkAudioFactory
    {
        private readonly IAudioStorage __Storage;
        private readonly IWebDownloader __Downloader;

        public VkAudioFactory(IAudioStorage storage, IWebDownloader downloader)
        {
            __Storage = storage;
            __Downloader = downloader;
        }

        public IAudio Create(long audioId, long userId, string title, string artist, int duration, Uri url)
        {
            return new VkAudio(__Storage, __Downloader, audioId, userId, title, artist, duration, url);
        }
    }
}
