using System;
using System.ComponentModel;
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

            var storage = new IsolatedStorageFileStorage();
            var login = new p_Login(userId => AccountManager.Instance.Init(userId, storage, web));
            login.PropertyChanged += LoginOnPropertyChanged;
            Content = login;
        }

        private void LoginOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "LoggedIn")
            {
                Content = new p_Player(AccountManager.Instance.AudioInfoProvider,
                    AccountManager.Instance.AudioInfoCacheOnlyProvider,
                    AccountManager.Instance.UserFirstName);
            }
        }
    }
}
