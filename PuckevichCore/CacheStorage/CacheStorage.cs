using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PuckevichCore.Interfaces;

namespace PuckevichCore.CacheStorage
{
    internal class CacheStorage : IAudioStorage
    {
        private readonly IFileStorage __Storage;
        private const string MAP_FILE = "audios.json";
        private const string FILE_NAME_PATTERN = "{0} - {1}#{2}#.mp3";

        private readonly Dictionary<long, JsonAudioModel> __AudioDict = new Dictionary<long, JsonAudioModel>();
        private readonly Dictionary<long, SortedList<int, long>> __AudioIdList = new Dictionary<long, SortedList<int, long>>();
        private readonly JsonSerializer __Serializer;
        private JsonTextWriter __Writer;

        public CacheStorage(IFileStorage storage)
        {
            __Storage = storage;

            var s = !__Storage.FileExists(MAP_FILE)
                           ? __Storage.CreateFile(MAP_FILE)
                           : __Storage.OpenFile(MAP_FILE, FileMode.Open);

            using (var file = new JsonTextReader(new StreamReader(s)))
            {
                __Serializer = new JsonSerializer { Formatting = Formatting.Indented };
                __AudioDict = __Serializer.Deserialize<Dictionary<long, JsonAudioModel>>(file) ?? new Dictionary<long, JsonAudioModel>();
            }

            foreach (var jsonAudioModel in __AudioDict)
            {
                if (!__AudioIdList.ContainsKey(jsonAudioModel.Value.UserId))
                    __AudioIdList.Add(jsonAudioModel.Value.UserId, new SortedList<int, long>());
                __AudioIdList[jsonAudioModel.Value.UserId].Add(jsonAudioModel.Value.Index, jsonAudioModel.Key);
            }
        }

        private string MakeFileName(JsonAudioModel model)
        {
            return String.Format(FILE_NAME_PATTERN, model.Artist, model.Title, model.AudioId);
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
                __Writer = new JsonTextWriter(new StreamWriter(__Storage.OpenFile(MAP_FILE, FileMode.Truncate)));
                __Serializer.Serialize(__Writer, __AudioDict);
            }
            finally
            {
                __Writer.Close();
                __Storage.Dispose();
            }
        }
    }
}