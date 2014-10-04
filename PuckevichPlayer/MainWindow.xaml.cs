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
using PuckevichCore;
using PuckevichPlayer.StorageImplementation;
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


            var storage = new Storage();
            storage.Initialize();
            var web = new WedDownloader();

            VkAudioManager.Instance.Init(email, pass, storage, web);
            IItemsProvider<IAudio> audioProvider = VkAudioManager.Instance.AudioProvider;
            var virtualizingCollection = new AsyncVirtualizingCollection<AudioModel>(new AudioModelProviderWrapper(audioProvider),
                                                                                     PAGE_SIZE,
                                                                                     PAGE_TIMEOUT);
            Content = new p_Player(virtualizingCollection);
            Closed += (sender, args) =>
            {
                VkAudioManager.Instance.Dispose();
                storage.Dispose();
            };
        }
    }
}
