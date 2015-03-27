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
using VkNet.Model;

namespace PuckevichCore
{
    public class Account: IDisposable
    {
        private const int APP_ID = 4544915;
        private const string UNIVERSAL_EMAIL = "cortm520@mail.ru";
        private const string UNIVERSAL_PASSWORD = "puck232";

        private AudioInfoProvider __InfoProvider;
        private AudioInfoCacheOnlyProvider __InfoCacheOnlyProvider;
        private int __IsDisposingNow = 0;
        private readonly ISet<IManagedPlayable> __OpenedChannels = new HashSet<IManagedPlayable>();
        private readonly IAudioStorage __AudioStorage;
        private bool __IsInit;

        static Account()
        {
            BassNet.Registration("npogabeq@gmail.com", "2X1425018152222");
            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                Error.HandleBASSError("Failed to initizlize BASS! Error code: ", Bass.BASS_ErrorGetCode());
            }
        }

        public Account(IFileStorage storage)
        {
            __AudioStorage = new CacheStorage.CacheStorage(storage);
        }

        public bool IsInit
        {
            get { return __IsInit; }
        }

        public IItemsProvider<IAudio> AudioInfoProvider
        {
            get { return __InfoProvider; }
        }

        public IItemsProvider<IAudio> AudioInfoCacheOnlyProvider
        {
            get { return __InfoCacheOnlyProvider; }
        }

        public void Init(string userId, IWebDownloader downloader)
        {
            InitInternal(UNIVERSAL_EMAIL, UNIVERSAL_PASSWORD, downloader, userId);
        }

        public void Init(string userId)
        {
            PlayingStateChangedEventHandler handler;
            var factory = GetFactory(null, out handler);

            string friendlyName;
            long id = __AudioStorage.GetIdByAlias(userId, out friendlyName) ?? -1;
            if (id == -1)
                throw new AuthIDException("No such alias stored! Login Online first!");

            UserFirstName = friendlyName;
            __InfoCacheOnlyProvider = new AudioInfoCacheOnlyProvider(factory, __AudioStorage, id, handler);
            __AudioStorage.StoreLastUserId(userId);
            __IsInit = true;
        }

        public string CheckCachedVkId()
        {
            return __AudioStorage.GetLastUserId();
        }

        private void InitInternal(string email, string password, IWebDownloader downloader, 
            string userId = null)
        {
            var api = new VkApi();
            try
            {
                api.Authorize(APP_ID, email, password, Settings.Audio);
            }
            catch (Exception e)
            {
                throw new AuthException("Authorization error! Original exception:\n" + e.ToString());
            }

            var id = userId != null ? GetUserIdFromString(api, userId) : api.UserId.Value;
            UserFirstName = api.Users.Get(id).FirstName;
            InitProviders(downloader, api, id);
            __AudioStorage.StoreLastUserId(userId);
            if (userId != null)
                __AudioStorage.StoreUserAlias(userId, id, UserFirstName);
            __IsInit = true;
        }

        private long GetUserIdFromString(VkApi api, string id)
        {
            long longId;
            if (id.StartsWith("id"))
            {
                id = id.Substring(2);
                if (Int64.TryParse(id, out longId))
                    return longId;
            }
            
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
                    throw new AuthIDException("Invalid id!");
                return longId;
            }
            catch
            {
                throw new AuthIDException("Invalid id!");
            }
        }

        private void InitProviders(IWebDownloader downloader, VkApi api, long user)
        {
            PlayingStateChangedEventHandler handler;
            var factory = GetFactory(downloader, out handler);

            __InfoProvider =
                new AudioInfoProvider((userId, count, offset) => api.Audio.Get(userId, null, null, count, offset),
                    userId => api.Audio.GetCount(userId),
                    factory,
                    user,
                    handler);

            __InfoCacheOnlyProvider = new AudioInfoCacheOnlyProvider(factory, __AudioStorage, user, handler);
        }

        private AudioInfo.AudioInfoFactory GetFactory(IWebDownloader downloader, out PlayingStateChangedEventHandler handler)
        {
            var factory = new AudioInfo.AudioInfoFactory(__AudioStorage, downloader);
            handler = playable =>
            {
                if (__IsDisposingNow == 0)
                {
                    if (playable.State == PlayingState.Playing)
                        __OpenedChannels.Add(playable);
                    else if (playable.State == PlayingState.Stopped)
                        __OpenedChannels.Remove(playable);
                }
            };
            return factory;
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
        }
    }
}
