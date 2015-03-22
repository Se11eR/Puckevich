using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using PuckevichCore.CacheStorage;
using PuckevichCore.DataVirtualization;
using PuckevichCore.Exceptions;
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
        private AudioInfoCacheOnlyProvider __InfoCacheOnlyProvider;
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

        public IItemsProvider<IAudio> AudioInfoCacheOnlyProvider
        {
            get { return __InfoCacheOnlyProvider; }
        }

        public void Init(string userId, IFileStorage storage, IWebDownloader downloader)
        {
            InitInternal(UNIVERSAL_EMAIL, UNIVERSAL_PASSWORD, storage, downloader, userId);
        }

        public void Init(string email, string password, IFileStorage storage, IWebDownloader downloader)
        {
            InitInternal(email, password, storage, downloader);
        }

        private void InitInternal(string email, string password, IFileStorage storage, IWebDownloader downloader, 
            string userId = null)
        {
            var api = new VkApi();
            api.Authorize(APP_ID, email, password, Settings.Audio);

            var id = userId != null ? GetUserIdFromString(api, userId) : api.UserId.Value;
            
            __AudioStorage = new CacheStorage.CacheStorage(storage);

            UserFirstName = api.Users.Get(id).FirstName;
            InitProviders(downloader, api, id);

            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                throw new Exception("Failed to initizlize BASS! Error code: " + Bass.BASS_ErrorGetCode());
            }
        }

        private long GetUserIdFromString(VkApi api, string id)
        {
            long longId;
            var dict = new Dictionary<string, string>();
            dict.Add("user_ids", id);
            try
            {
                var res = api.Invoke("users.get", dict, true);

                var serializer = new JsonSerializer();
                dynamic resDict =
                    serializer.Deserialize(new JsonTextReader(new StringReader(res)));

                id = resDict["response"][0]["uid"];
                if (!Int64.TryParse(id, out longId))
                    throw new AuthException("Invalid id!");
                return longId;
            }
            catch
            {
                if (id.StartsWith("id"))
                    id = id.Substring(2);
                if (!Int64.TryParse(id, out longId))
                    throw new AuthException("Invalid id!");
                return longId;
            }
        }

        private void InitProviders(IWebDownloader downloader, VkApi api, long user)
        {
            var factory = new AudioInfo.AudioInfoFactory(__AudioStorage, downloader);
            PlayingStateChangedEventHandler handler = playable =>
            {
                if (__IsDisposingNow == 0)
                {
                    if (playable.State == PlayingState.Playing)
                        __OpenedChannels.Add(playable);
                    else if (playable.State == PlayingState.Stopped)
                        __OpenedChannels.Remove(playable);
                }
            };

            __InfoProvider =
                new AudioInfoProvider((userId, count, offset) => api.Audio.Get(userId, null, null, count, offset),
                    userId => api.Audio.GetCount(userId),
                    factory,
                    user,
                    handler);

            __InfoCacheOnlyProvider = new AudioInfoCacheOnlyProvider(factory, __AudioStorage, user, handler);
        }

        public string UserFirstName { get; private set; }

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
