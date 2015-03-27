using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PuckevichCore.Interfaces;
using VkNet.Model;

namespace PuckevichCore.CacheStorage
{
    internal class CacheStorage : IAudioStorage
    {
        private readonly IFileStorage __Storage;
        private const string MAP_FILE = "audios.json";
        private const string USERID_FILE = "lastuser.id";
        private const string AILASTOID_FILE = "aliastoid.json";
        private const string FILE_NAME_PATTERN = "{0}.mp3";

        private readonly Dictionary<long, JsonAudioModel> __AudioDict = new Dictionary<long, JsonAudioModel>();

        private readonly Dictionary<long, SortedList<int, long>> __AudioIdList =
            new Dictionary<long, SortedList<int, long>>();

        private readonly Dictionary<string, JsonUserModel> __AliasMap = new Dictionary<string, JsonUserModel>();
        private string __UserId = null;

        public CacheStorage(IFileStorage storage)
        {
            __Storage = storage;

            var s = !__Storage.FileExists(MAP_FILE)
                ? __Storage.CreateFile(MAP_FILE)
                : __Storage.OpenFile(MAP_FILE, FileMode.Open);

            var serializer = new JsonSerializer { Formatting = Formatting.Indented };
            using (var file = new JsonTextReader(new StreamReader(s)))
            {
                __AudioDict = serializer.Deserialize<Dictionary<long, JsonAudioModel>>(file) ??
                              new Dictionary<long, JsonAudioModel>();
            }

            foreach (var jsonAudioModel in __AudioDict)
            {
                if (!__AudioIdList.ContainsKey(jsonAudioModel.Value.UserId))
                    __AudioIdList.Add(jsonAudioModel.Value.UserId, new SortedList<int, long>());
                __AudioIdList[jsonAudioModel.Value.UserId].Add(jsonAudioModel.Value.Index, jsonAudioModel.Key);
            }

            if (__Storage.FileExists(USERID_FILE))
            {
                try
                {
                    using (var file = new StreamReader(__Storage.OpenFile(USERID_FILE, FileMode.Open)))
                    {
                        __UserId = file.ReadToEnd();
                    }
                }
                catch
                {
                    //Ќичего не делать, последнего сохраненного алиаса не будет.
                }
            }

            if (__Storage.FileExists(AILASTOID_FILE))
            {
                try
                {
                    using (var file = new JsonTextReader(new StreamReader(__Storage.OpenFile(AILASTOID_FILE, FileMode.Open))))
                    {
                        __AliasMap = serializer.Deserialize<Dictionary<string, JsonUserModel>>(file);
                    }
                }
                catch
                {
                    //Ќичего не делать, карты алиасов не будет.
                }
            }
        }

        private string MakeFileName(JsonAudioModel model)
        {
            return String.Format(FILE_NAME_PATTERN, model.AudioId);
        }

        private ICacheStream LocateCacheStream(JsonAudioModel audio)
        {
            var isoStream = __Storage.OpenFile(MakeFileName(audio), FileMode.Open);
            ICacheStream s = new CacheStream(isoStream, audio);

            s.AudioSize = audio.AudioSize;
            s.Position = isoStream.Length;
            return s;
        }

        private ICacheStream CreateCacheStream(IAudio audio)
        {
            var audioModel = new JsonAudioModel()
            {
                Artist = audio.Artist,
                AudioId = audio.AudioId,
                Duration = audio.Duration,
                Title = audio.Title,
                UserId = audio.UserId,
                Index = audio.Index
            };

            __AudioDict.Add(audioModel.AudioId, audioModel);

            if (!__AudioIdList.ContainsKey(audio.UserId))
                __AudioIdList.Add(audio.UserId, new SortedList<int, long>());

            __AudioIdList[audioModel.UserId].Add(audioModel.Index, audioModel.AudioId);
            ICacheStream s = new CacheStream(__Storage.CreateFile(MakeFileName(audioModel)), audioModel);

            return s;
        }

        public ICacheStream GetCacheStream(IAudio audio)
        {
            JsonAudioModel audioModel;
            ICacheStream s;
            if (__AudioDict.TryGetValue(audio.AudioId, out audioModel))
            {
                if (__Storage.FileExists(MakeFileName(audioModel)))
                {
                    s = LocateCacheStream(audioModel);
                }
                else
                {
                    s = CreateCacheStream(audio);
                }
            }
            else
                s = CreateCacheStream(audio);

            return s;
        }

        public IAudio GetAt(long userId, int index)
        {
            JsonAudioModel audioModel;
            var selectedUserSortedList = __AudioIdList[userId];
            if (__AudioDict.TryGetValue(selectedUserSortedList[selectedUserSortedList.Keys[index]], out audioModel))
            {
                if (__Storage.FileExists(MakeFileName(audioModel)))
                {
                    return audioModel;
                }
            }

            return null;
        }

        public bool CheckCached(IAudio audio)
        {
            JsonAudioModel audioModel;
            if (__AudioDict.TryGetValue(audio.AudioId, out audioModel))
            {
                var fname = MakeFileName(audioModel);
                if (__Storage.FileExists(fname))
                {
                    var length = __Storage.GetFileSize(fname);
                    return length > 0 && length >= audioModel.AudioSize;
                }
                return false;
            }
            return false;
        }

        public void RemovecachedAudio(long auidiId)
        {
            throw new NotImplementedException();
        }

        public void StoreLastUserId(string userId)
        {
            __UserId = userId;
        }

        public string GetLastUserId()
        {
            return __UserId;
        }

        public void StoreUserAlias(string alias, long id, string userFriendlyName)
        {
            if (!__AliasMap.ContainsKey(alias))
                __AliasMap.Add(alias, new JsonUserModel() {FriendlyName = userFriendlyName, UserId = id});
            else
                __AliasMap[alias] = new JsonUserModel() {FriendlyName = userFriendlyName, UserId = id};
        }

        public long? GetIdByAlias(string alias, out string userFriendlyName)
        {
            userFriendlyName = null;
            JsonUserModel model;
            if (!__AliasMap.TryGetValue(alias, out model))
                return null;
            userFriendlyName = model.FriendlyName;
            return model.UserId;
        }

        public int GetCount(long userId)
        {
            if (!__AudioIdList.ContainsKey(userId))
                __AudioIdList.Add(userId, new SortedList<int, long>());

            return __AudioIdList[userId].Keys.Count;
        }

        public void Dispose()
        {
            try
            {
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                using (
                    var writer = new JsonTextWriter(new StreamWriter(__Storage.OpenFile(MAP_FILE, FileMode.Truncate))))
                {
                    serializer.Serialize(writer, __AudioDict);
                }
                var aliasFileExists = __Storage.FileExists(AILASTOID_FILE);
                using (
                    var writer =
                        new JsonTextWriter(
                            new StreamWriter(__Storage.OpenFile(AILASTOID_FILE,
                                aliasFileExists ? FileMode.Truncate : FileMode.CreateNew))))
                {
                    serializer.Serialize(writer, __AliasMap);
                }

                if (__UserId != null)
                {
                    var userIdFileExists = __Storage.FileExists(USERID_FILE);
                    using (
                        var file =
                            new StreamWriter(__Storage.OpenFile(USERID_FILE,
                                userIdFileExists ? FileMode.Truncate : FileMode.CreateNew)))
                    {
                        file.Write(__UserId);
                    }
                }
            }
            finally
            {
                __Storage.Dispose();
            }
        }
    }
}