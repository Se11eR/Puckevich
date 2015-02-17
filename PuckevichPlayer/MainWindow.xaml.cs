using System;
using System.Linq;
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
using PuckevichPlayer.Pages;
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

        private CacheStorage __Storage;

        public MainWindow()
        {
            InitializeComponent();

            Content = new p_Start();
        }

        private void OnClosed(object sender, EventArgs args)
        {
            AudioManager.Instance.Dispose();
            __Storage.Dispose();
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            __Storage = new CacheStorage();
            var web = new WedDownloader();

            await Task.Run(() =>
            {
                __Storage.Initialize();
                AudioManager.Instance.Init(email, pass, __Storage, web);
                
            });

            IItemsProvider<IAudio> audioProvider = AudioManager.Instance.AudioInfoProvider;
            var collection =
                new AsyncVirtualizingCollection<AudioModel>(new AudioModelProviderWrapper(audioProvider),
                                                            PAGE_SIZE,
                                                            PAGE_TIMEOUT);
            Content = new p_Player(collection);
        }
    }
}
