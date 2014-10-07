using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PuckevichCore;

namespace PuckevichPlayer.Storage
{
    public class CacheStorage : IAudioStorage
    {
        private const string MAP_FILE = "audios.json";
        private const string FILE_NAME_PATTERN = "{0} - {1}#{2}#.mp3";

        private IsolatedStorageFile __IsoStorage;
        private Dictionary<long, JsonAudioModel> __AudioDict = new Dictionary<long, JsonAudioModel>();
        private JsonTextWriter __Writer;
        private JsonSerializer __Serializer;

        private string MakeFileName(JsonAudioModel model)
        {
            return String.Format(FILE_NAME_PATTERN, model.Artist, model.Title, model.AudioId);
        }

        private ICacheStream LocateCacheStream(JsonAudioModel audio)
        {
            var isoStream = new IsolatedStorageFileStream(MakeFileName(audio), FileMode.Open);
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
            };

            __AudioDict.Add(audioModel.AudioId, audioModel);
            ICacheStream s = new CacheStream(new IsolatedStorageFileStream(MakeFileName(audioModel), FileMode.Create), audioModel);

            return s;
        }

        private async Task<ICacheStream> CreateCacheStreamAsync(IAudio audio)
        {
            return await Task.Run(() => CreateCacheStream(audio));
        }

        public async Task<ICacheStream> GetCacheStreamAsync(IAudio audio)
        {
            JsonAudioModel audioModel;
            ICacheStream s;
            if (__AudioDict.TryGetValue(audio.AudioId, out audioModel))
            {
                if (__IsoStorage.FileExists(MakeFileName(audioModel)))
                {
                    s = await LocateCacheStreamAsync(audioModel);
                }
                else
                {
                    s = await CreateCacheStreamAsync(audio);
                }
            }
            else
                s = await CreateCacheStreamAsync(audio);

            return s;
        }

        public ICacheStream GetCacheStream(IAudio audio)
        {
            JsonAudioModel audioModel;
            ICacheStream s;
            if (__AudioDict.TryGetValue(audio.AudioId, out audioModel))
            {
                if (__IsoStorage.FileExists(MakeFileName(audioModel)))
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

        public void Initialize()
        {
            __IsoStorage =
                IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly,
                                             null,
                                             null);

            Stream s = !__IsoStorage.FileExists(MAP_FILE)
                           ? __IsoStorage.CreateFile(MAP_FILE)
                           : __IsoStorage.OpenFile(MAP_FILE, FileMode.Open);

            using (var file = new JsonTextReader(new StreamReader(s)))
            {
                __Serializer = new JsonSerializer { Formatting = Formatting.Indented };
                __AudioDict = __Serializer.Deserialize<Dictionary<long, JsonAudioModel>>(file) ?? new Dictionary<long, JsonAudioModel>();
            }
        }

        public void RemovecachedAudio(long auidiId)
        {
            __AudioDict.Remove(auidiId);
        }

        public void Dispose()
        {
            try
            {
                __Writer = new JsonTextWriter(new StreamWriter(__IsoStorage.OpenFile(MAP_FILE, FileMode.Truncate)));
                __Serializer.Serialize(__Writer, __AudioDict);
            }
            finally
            {
                __Writer.Close();
                __IsoStorage.Close();
            }
        }
    }
}