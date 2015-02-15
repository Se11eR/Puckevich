﻿using System;
using VkNet.Model;

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

        public IAudio Create(long audioId, long userId, string title, string artist, int duration, Uri url)
        {
            return new AudioInfo(__Storage, __Downloader, audioId, userId, title, artist, duration, url);
        }
    }
}