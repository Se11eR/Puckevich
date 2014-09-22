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

        private IItemsProvider<IAudio> __AudioProvider;

        public MainWindow()
        {
            InitializeComponent();


            var storage = new Storage();
            storage.Initialize();
            var web = new WedDownloader();

            var manager = new VkAudioManager(email, pass, storage, web);
            __AudioProvider = manager.AudioProvider;
            var virtualizingCollection = new AsyncVirtualizingCollection<AudioModel>(new AudioModelProviderWrapper(__AudioProvider),
                                                                                     PAGE_SIZE,
                                                                                     PAGE_TIMEOUT);
            Content = new p_Player(virtualizingCollection);
        }
    }
    public class Container : IStoredAudioContainer
    {
        public Stream CachedStream
        {
            get;
            set;
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
        public readonly string AppName = "PuckevichPlayer";

        private readonly Dictionary<long, Tuple<IStoredAudioContainer, AudioStorageStatus>> __Storage =
            new Dictionary<long, Tuple<IStoredAudioContainer, AudioStorageStatus>>();

        private IsolatedStorageFile __IsoStorage;

        public void Initialize()
        {
            __IsoStorage =
                IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly,
                                             null,
                                             null);
        }

        public IStoredAudioContainer StoreAudio(long audioId)
        {
            if (__Storage.ContainsKey(audioId))
                throw new InvalidOperationException();

            var container = new Container();
            container.CachedStream = __IsoStorage.CreateFile(audioId + ".mp3");

            var tuple = new Tuple<IStoredAudioContainer, AudioStorageStatus>(container, AudioStorageStatus.PartiallyStored);
            __Storage.Add(audioId, tuple);

            return tuple.Item1;
        }

        public IStoredAudioContainer GetAudio(long audioId)
        {
            return __Storage[audioId].Item1;
        }

        public AudioStorageStatus GetStatus(long audioId)
        {
            if (__Storage.ContainsKey(audioId))
                return __Storage[audioId].Item2;

            return AudioStorageStatus.NotStored;
        }

        public void SetStatus(long audioId, AudioStorageStatus status)
        {
            if (__Storage.ContainsKey(audioId))
                __Storage[audioId] = new Tuple<IStoredAudioContainer, AudioStorageStatus>(__Storage[audioId].Item1, status);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
