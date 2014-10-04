using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PuckevichCore;

namespace PuckevichPlayer.StorageImplementation
{
    public class Storage : IAudioStorage
    {
        private const string MAP_FILE = "audios.json";
        private const string FILE_NAME_PATTERN = "{0} - {1}#{2}#.mp3";

        private IsolatedStorageFile __IsoStorage;
        private Dictionary<long, JsonAudioModel> __AudioDict = new Dictionary<long,JsonAudioModel>();
        private JsonTextWriter __Writer;
        private JsonSerializer __Serializer;

        private async Task UpdateFile()
        {
            await Task.Factory.StartNew(() =>__Serializer.Serialize(__Writer, __AudioDict));
        }

        private async Task<ICacheStream> LocateCacheStream(JsonAudioModel audio)
        {
            ICacheStream s =
                await
                Task.Factory.StartNew(
                                      () =>
                                      new CacheStream(
                                          __IsoStorage.OpenFile(String.Format(FILE_NAME_PATTERN, audio.Artist, audio.Title, audio.AudioId),
                                                                FileMode.Open),
                                          new Task<Task>(UpdateFile)));
            if (s != null)
                s.Position = s.Length ?? 1 - 1;
            return s;
        }

        private async Task<ICacheStream> CreateCacheStream(IAudio audio)
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
            ICacheStream s =
                await
                Task.Factory.StartNew(
                                      () =>
                                      new CacheStream(
                                          __IsoStorage.CreateFile(String.Format(FILE_NAME_PATTERN,
                                                                                audioModel.Artist,
                                                                                audioModel.Title,
                                                                                audioModel.AudioId)),
                                          new Task<Task>(UpdateFile)));
            await UpdateFile();


            s.Length = null;
            s.Position = 0;
            return s;
        }

        public async Task<ICacheStream> GetCacheStream(IAudio audio)
        {
            JsonAudioModel audioModel;
            ICacheStream s;
            if (__AudioDict.TryGetValue(audio.AudioId, out audioModel))
                s = await LocateCacheStream(audioModel);
            else
                s = await CreateCacheStream(audio);

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

            __Writer = new JsonTextWriter(new StreamWriter(__IsoStorage.OpenFile(MAP_FILE, FileMode.Open)));
        }

        public async Task RemovecachedAudio(long auidiId)
        {
            __AudioDict.Remove(auidiId);
            await UpdateFile();
        }

        public void Dispose()
        {
            __Writer.Close();
            __IsoStorage.Dispose();
        }
    }
}