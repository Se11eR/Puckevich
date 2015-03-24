using System;
using System.Collections.Generic;
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
using FluidKit.Controls;
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
        private PlayerLogin __PlayerLogin;
        private p_Login __LoginPage;
        private p_Player __PlayerPage;
        private WedDownloader __Downloader;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OnClosed(object sender, EventArgs args)
        {
            if (__PlayerLogin != null)
                __PlayerLogin.Dispose();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            __Downloader = new WedDownloader();
            CreateLogin();
        }

        private void CreateLogin()
        {
            if (Trans.Items.Contains(__LoginPage))
                Trans.Items.Remove(__LoginPage);
            if (__PlayerLogin != null)
            {
                __PlayerLogin.Dispose();
                __PlayerLogin = null;
            }

            __LoginPage = null;
            __LoginPage =
                new p_Login(
                    userId =>
                        __PlayerLogin = new PlayerLogin(userId, new IsolatedStorageFileStorage(), __Downloader));
            __LoginPage.PropertyChanged += LoginOnPropertyChanged;
            Trans.Items.Add(__LoginPage);
        }

        private void LoginOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "LoggedIn")
            {
                __PlayerPage = new p_Player(__PlayerLogin.AudioInfoProvider,
                    __PlayerLogin.AudioInfoCacheOnlyProvider,
                    NavigateBack,
                    __PlayerLogin.UserFirstName
                    );
                Trans.Items.Add(__PlayerPage);
                Trans.Transition = new SlideTransition(Direction.RightToLeft)
                {
                    Duration = new Duration(
                        TimeSpan.FromSeconds(0.3))
                };
                Trans.ApplyTransition(__LoginPage, __PlayerPage);
            }
        }


        private void NavigateBack()
        {
            CreateLogin();
            Trans.Transition = new SlideTransition(Direction.LeftToRight)
            {
                Duration = new Duration(
                    TimeSpan.FromSeconds(0.3))
            };
            Trans.ApplyTransition(__PlayerPage, __LoginPage);

            Trans.Items.Remove(__PlayerPage);
            __PlayerPage = null;
        }
    }
}
