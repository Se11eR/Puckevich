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
        private readonly SortedList<int, long> __AudioIdList = new SortedList<int, long>();
        private JsonTextWriter __Writer;
        private JsonSerializer __Serializer;

        public CacheStorage(IFileStorage storage)
        {
            __Storage = storage;

            Stream s = !__Storage.FileExists(MAP_FILE)
                           ? __Storage.CreateFile(MAP_FILE)
                           : __Storage.OpenFile(MAP_FILE, FileMode.Open);

            using (var file = new JsonTextReader(new StreamReader(s)))
            {
                __Serializer = new JsonSerializer { Formatting = Formatting.Indented };
                __AudioDict = __Serializer.Deserialize<Dictionary<long, JsonAudioModel>>(file) ?? new Dictionary<long, JsonAudioModel>();
            }
            foreach (var jsonAudioModel in __AudioDict)
            {
                __AudioIdList.Add(jsonAudioModel.Value.Index, jsonAudioModel.Key);
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

        private async Task<ICacheStream> LocateCacheStreamAsync(JsonAudioModel audio)
        {
            return await Task.Run(() => LocateCacheStream(audio));
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
            __AudioIdList.Add(audioModel.Index, audioModel.AudioId);
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

        public IAudio GetAt(int index)
        {
            JsonAudioModel audioModel;

            if (__AudioDict.TryGetValue(__AudioIdList[__AudioIdList.Keys[index]], out audioModel))
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
            __AudioDict.Remove(auidiId);
        }

        public int Count
        {
            get { return __AudioDict.Keys.Count; }
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