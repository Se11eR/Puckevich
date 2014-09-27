using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using PuckevichCore;

namespace PuckevichPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int PAGE_SIZE = 100;
        private const int PAGE_TIMEOUT = 1000 * 60; //1 minute

        public MainWindow()
        {
            InitializeComponent();


            var storage = new Storage();
            storage.Initialize();
            var web = new WedDownloader();

            var manager = new VkAudioManager(email, pass, storage, web);
            IItemsProvider<IAudio> audioProvider = manager.AudioProvider;
            var virtualizingCollection = new AsyncVirtualizingCollection<AudioModel>(new AudioModelProviderWrapper(audioProvider),
                                                                                     PAGE_SIZE,
                                                                                     PAGE_TIMEOUT);
            Content = new p_Player(virtualizingCollection);
            Closed += (sender, args) =>
            {
                manager.Dispose();
                storage.Dispose();
            };
        }
    }
    public class WedDownloader : IWebDownloader
    {
        public Stream GetUrlStream(Uri url, out long streamLength)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            var response = (HttpWebResponse) request.GetResponse();
            var resStream = response.GetResponseStream();
            streamLength = response.ContentLength;
            return resStream;
        }
    }

    public class Storage : IAudioStorage
    {
        private const string MAP_FILE = "audios.json";
        private const string FILE_NAME_TEMPLATE = "{0} - {1}#{2}#.mp3";

        private IsolatedStorageFile __IsoStorage;
        private Dictionary<long, JsonModel> __AudioDict = new Dictionary<long,JsonModel>();
        private JsonTextWriter __Writer;
        private JsonSerializer __Serializer;

        private class JsonModel
        {
            public long AudioId { get; set; }

            public long UserId { get; set; }

            public string Title { get; set; }

            public string Artist { get; set; }

            public int Duration { get; set; }

            public int Status { get; set; }
        }

        private void UpdateFile()
        {
            __Serializer.Serialize(__Writer, __AudioDict);
        }

        public void Initialize()
        {
            __IsoStorage =
                IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly,
                                             null,
                                             null);
            if (!__IsoStorage.FileExists(MAP_FILE))
            {
                __IsoStorage.CreateFile(MAP_FILE);
            }

            using (var file = new JsonTextReader(new StreamReader(__IsoStorage.OpenFile(MAP_FILE, FileMode.Open))))
            {
                __Serializer = new JsonSerializer {Formatting = Formatting.Indented};
                __AudioDict = __Serializer.Deserialize<Dictionary<long, JsonModel>>(file);
            }

            __Writer = new JsonTextWriter(new StreamWriter(__IsoStorage.OpenFile(MAP_FILE, FileMode.Open)));
        }

        public Stream CreateCacheStream(IAudio audio)
        {
            JsonModel model;
            Stream s;
            if (!__AudioDict.TryGetValue(audio.AudioId, out model))
            {
                model = new JsonModel()
                {
                    Artist = audio.Artist,
                    AudioId = audio.AudioId,
                    Duration = audio.Duration,
                    Title = audio.Title,
                    UserId = audio.UserId,
                    Status = (int)AudioStorageStatus.PartiallyStored
                };

                __AudioDict.Add(model.AudioId, model);
                s = __IsoStorage.CreateFile(String.Format(FILE_NAME_TEMPLATE, model.Artist, model.Title, model.AudioId));
                UpdateFile();
            }
            else
                s = __IsoStorage.OpenFile(String.Format(FILE_NAME_TEMPLATE, model.Artist, model.Title, model.AudioId),
                    FileMode.Open);

            return s;
        }

        public Stream LookupCacheStream(long audioId)
        {
            JsonModel model;
            return !__AudioDict.TryGetValue(audioId, out model)
                ? null
                : __IsoStorage.OpenFile(String.Format(FILE_NAME_TEMPLATE, model.Artist, model.Title, model.AudioId),
                    FileMode.Open);
        }

        public void RemovecachedAudio(long auidiId)
        {
            __AudioDict.Remove(auidiId);
            UpdateFile();
        }

        public AudioStorageStatus GetStatus(long audioId)
        {
            JsonModel model;
            if (!__AudioDict.TryGetValue(audioId, out model))
            {
                return AudioStorageStatus.NotStored;
            }
            return (AudioStorageStatus)model.Status;
        }

        public void SetStatus(long audioId, AudioStorageStatus status)
        {
            JsonModel model;
            if (!__AudioDict.TryGetValue(audioId, out model))
                return;

            model.Status = (int)status;
            UpdateFile();
        }

        public void Dispose()
        {
            __Writer.Close();
            __IsoStorage.Dispose();
        }
    }
}
