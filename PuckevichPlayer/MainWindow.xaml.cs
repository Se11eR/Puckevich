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
using PuckevichCore.DataVirtualization;
using PuckevichCore.Interfaces;
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

        public MainWindow()
        {
            InitializeComponent();

            Content = new p_Start();
        }

        private void OnClosed(object sender, EventArgs args)
        {
            AccountManager.Instance.Dispose();
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var web = new WedDownloader();
            string email = "vkontakt232@gmail.com";
            string pass = "ohmaniwillneverforgiveyourassforthisshit";

            await Task.Run(() => AccountManager.Instance.Init(email, pass, new IsolatedStorageFileStorage(), web));

            IItemsProvider<IAudio> audioProvider = AccountManager.Instance.AudioInfoProvider;
            var collection =
                new AsyncVirtualizingCollection<AudioModel>(new AudioModelProviderWrapper(audioProvider),
                                                            PAGE_SIZE,
                                                            PAGE_TIMEOUT);
            Content = new p_Player(collection, AccountManager.Instance.UserFirstName);
        }
    }
}
