using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PuckevichPlayer.Storage;
using PuckevichCore;
using PuckevichPlayer.Virtualizing;

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

            string email = "vkontakt232@gmail.com";
            string pass = "ohmaniwillneverforgiveyourassforthisshit";

            var storage = new CacheStorage();
            storage.Initialize();
            var web = new WedDownloader();

            AudioManager.Instance.Init(email, pass, storage, web);
            IItemsProvider<IAudio> audioProvider = AudioManager.Instance.AudioInfoProvider;
            var virtualizingCollection = new AsyncVirtualizingCollection<AudioModel>(new AudioModelProviderWrapper(audioProvider),
                                                                                        PAGE_SIZE,
                                                                                        PAGE_TIMEOUT);

            Content = new p_Player(virtualizingCollection);
            Closed += (sender, args) =>
            {
                AudioManager.Instance.Dispose();
                storage.Dispose();
            };
        }


        static void DownloadFile(string sourceUrl, string destinationPath)
        {
            long iFileSize = 0;
            int iBufferSize = 1024;
            iBufferSize *= 1000;
            long iExistLen = 0;
            System.IO.FileStream saveFileStream;
            if (System.IO.File.Exists(destinationPath))
            {
                System.IO.FileInfo fINfo =
                   new System.IO.FileInfo(destinationPath);
                iExistLen = fINfo.Length;
            }
            if (iExistLen > 0)
                saveFileStream = new System.IO.FileStream(destinationPath,
                  System.IO.FileMode.Append, System.IO.FileAccess.Write,
                  System.IO.FileShare.ReadWrite);
            else
                saveFileStream = new System.IO.FileStream(destinationPath,
                  System.IO.FileMode.Create, System.IO.FileAccess.Write,
                  System.IO.FileShare.ReadWrite);

            System.Net.HttpWebRequest hwRq;
            System.Net.HttpWebResponse hwRes;
            hwRq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(sourceUrl);
            hwRq.AddRange((int)iExistLen);
            System.IO.Stream smRespStream;
            hwRes = (System.Net.HttpWebResponse)hwRq.GetResponse();
            smRespStream = hwRes.GetResponseStream();

            iFileSize = hwRes.ContentLength;

            int iByteSize;
            byte[] downBuffer = new byte[iBufferSize];

            while ((iByteSize = smRespStream.Read(downBuffer, 0, downBuffer.Length)) > 0)
            {
                saveFileStream.Write(downBuffer, 0, iByteSize);
            }
        }  
    }
}
