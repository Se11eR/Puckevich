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
        private Dictionary<long, JsonAudioModel> __AudioDict = new Dictionary<long,JsonAudioModel>();
        private JsonTextWriter __Writer;
        private JsonSerializer __Serializer;

        private async Task UpdateFileAsync()
        {
            await Task.Factory.StartNew(() =>__Serializer.Serialize(__Writer, __AudioDict));
        }

        private void UpdateFile()
        {
            __Serializer.Serialize(__Writer, __AudioDict);
        }

        private ICacheStream LocateCacheStream(JsonAudioModel audio)
        {
            ICacheStream s =
                new CacheStream(
                    __IsoStorage.OpenFile(String.Format(FILE_NAME_PATTERN, audio.Artist, audio.Title, audio.AudioId), FileMode.Open),
                    new Task<Task>(UpdateFileAsync),
                    UpdateFile);
            s.Position = s.Length ?? 1 - 1;
            return s;
        }

        private async Task<ICacheStream> LocateCacheStreamAsync(JsonAudioModel audio)
        {
            ICacheStream s =
                await
                Task.Factory.StartNew(
                                      () =>
                                      new CacheStream(
                                          __IsoStorage.OpenFile(String.Format(FILE_NAME_PATTERN, audio.Artist, audio.Title, audio.AudioId),
                                                                FileMode.Open),
                                          new Task<Task>(UpdateFileAsync),
                                          UpdateFile));
            if (s != null)
                s.Position = s.Length ?? 1 - 1;
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
            };

            __AudioDict.Add(audioModel.AudioId, audioModel);
            ICacheStream s =
                new CacheStream(
                    __IsoStorage.CreateFile(String.Format(FILE_NAME_PATTERN, audioModel.Artist, audioModel.Title, audioModel.AudioId)),
                    new Task<Task>(UpdateFileAsync),
                    UpdateFile);
            UpdateFile();

            s.Length = null;
            s.Position = 0;
            return s;
        }

        private async Task<ICacheStream> CreateCacheStreamAsync(IAudio audio)
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
                                          new Task<Task>(UpdateFileAsync),
                                          UpdateFile));
            await UpdateFileAsync();


            s.Length = null;
            s.Position = 0;
            return s;
        }

        public async Task<ICacheStream> GetCacheStreamAsync(IAudio audio)
        {
            JsonAudioModel audioModel;
            ICacheStream s;
            if (__AudioDict.TryGetValue(audio.AudioId, out audioModel))
                s = await LocateCacheStreamAsync(audioModel);
            else
                s = await CreateCacheStreamAsync(audio);

            return s;
        }

        public ICacheStream GetCacheStream(IAudio audio)
        {
            JsonAudioModel audioModel;
            ICacheStream s;
            s = __AudioDict.TryGetValue(audio.AudioId, out audioModel) ? LocateCacheStream(audioModel) : CreateCacheStream(audio);

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

        public async Task RemovecachedAudioAsync(long auidiId)
        {
            __AudioDict.Remove(auidiId);
            await UpdateFileAsync();
        }

        public void RemovecachedAudio(long auidiId)
        {
            __AudioDict.Remove(auidiId);
            UpdateFile();
        }

        public void Dispose()
        {
            __Writer.Close();
            __IsoStorage.Close();
        }
    }
}