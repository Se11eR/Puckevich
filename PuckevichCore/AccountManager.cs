﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using PuckevichCore.DataVirtualization;
using PuckevichCore.Interfaces;
using Un4seen.Bass;
using VkNet;
using VkNet.Enums.Filters;

namespace PuckevichCore
{
    public class AccountManager: IDisposable
    {
        private const int APP_ID = 4544915;
        private const string UNIVERSAL_EMAIL = "cortm520@mail.ru";
        private const string UNIVERSAL_PASSWORD = "puck232";


        private static readonly Object __SingletoneLock = new Object();
        private static AccountManager __Instance;

        private AudioInfoProvider __InfoProvider;
        private int __IsDisposingNow = 0;
        private readonly ISet<IManagedPlayable> __OpenedChannels = new HashSet<IManagedPlayable>();
        private IAudioStorage __AudioStorage;


        private AccountManager()
        {
        }

        public static AccountManager Instance
        {
            get
            {
                if (__Instance != null) return __Instance;
                Monitor.Enter(__SingletoneLock);
                var temp = new AccountManager();
                Interlocked.Exchange(ref __Instance, temp);
                Monitor.Exit(__SingletoneLock);
                return __Instance;
            }
        }

        public IItemsProvider<IAudio> AudioInfoProvider
        {
            get { return __InfoProvider; }
        }

        public void Init(long userId, IFileStorage storage, IWebDownloader downloader)
        {
            InitInternal(UNIVERSAL_EMAIL, UNIVERSAL_PASSWORD, storage, downloader, userId);
        }

        public void Init(string email, string password, IFileStorage storage, IWebDownloader downloader)
        {
            InitInternal(email, password, storage, downloader);
        }

        private void InitInternal(string email, string password, IFileStorage storage, IWebDownloader downloader, long? userId = null)
        {
            var api = new VkApi();
            api.Authorize(APP_ID, email, password, Settings.Audio);
            __AudioStorage = new CacheStorage.CacheStorage(storage);

            __InfoProvider = GetInfoProvider(downloader, api, userId ?? api.UserId.Value);

            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                throw new Exception("Failed to initizlize BASS! Error code: " + Bass.BASS_ErrorGetCode());
            }
        }

        private AudioInfoProvider GetInfoProvider(IWebDownloader downloader, VkApi api, long user)
        {
            return new AudioInfoProvider((userId, count, offset) => api.Audio.Get(userId, null, null, count, offset),
                userId => api.Audio.GetCount(userId),
                new AudioInfoFactory(__AudioStorage, downloader),
                user,
                playable =>
                {
                    if (__IsDisposingNow == 0)
                    {
                        if (playable.State == PlayingState.Playing)
                            __OpenedChannels.Add(playable);
                        else if (playable.State == PlayingState.Stopped)
                            __OpenedChannels.Remove(playable);
                    }
                });
        }

        public void Dispose()
        {
            Interlocked.Increment(ref __IsDisposingNow);
            foreach (var audio in __OpenedChannels)
            {
                audio.Stop();
            }
            if (__AudioStorage != null)
                __AudioStorage.Dispose();
            Bass.BASS_Free();
        }
    }
}