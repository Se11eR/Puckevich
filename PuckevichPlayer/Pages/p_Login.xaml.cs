using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using PuckevichCore.Exceptions;

namespace PuckevichPlayer.Pages
{
    /// <summary>
    /// Interaction logic for p_Login.xaml
    /// </summary>
    public partial class p_Login : UserControl, INotifyPropertyChanged
    {
        private readonly Action<string> __OnlineLogin;
        private readonly Action<string> __CacheLogin;
        private bool __LoggingIn;
        private bool __LoggedIn;
        private string __ErrorMessage;

        public p_Login(string lastId, bool immediateLogin, Action<string> onlineLogin, Action<string> cacheLogin)
        {
            InitializeComponent();
            DataContext = this;

            UserVkId = lastId;
            __OnlineLogin = onlineLogin;
            __CacheLogin = cacheLogin;

            if (lastId != null && immediateLogin)
                OnlineLogin_Click(null, null);
        }

        public bool LoggingIn
        {
            get { return __LoggingIn; }
            set
            {
                __LoggingIn = value;
                OnPropertyChanged();
            }
        }

        public string UserVkId { get; set; }

        public bool LoggedIn
        {
            get { return __LoggedIn; }
            set
            {
                __LoggedIn = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get { return __ErrorMessage; }
            set
            {
                __ErrorMessage = value;
                OnPropertyChanged();
            }
        }

        private async void OnlineLogin_Click(object sender, RoutedEventArgs e)
        {
            LoggingIn = true;
            try
            {
                await Task.Run(() => __OnlineLogin(UserVkId));

                ErrorMessage = null;
                LoggedIn = true;
            }
            catch (AuthIDException)
            {
                ErrorMessage = "No such page in cache. You may need to login online first.";
            }
            catch (AuthException)
            {
                ErrorMessage = "Error occured.";
            }
            finally
            {
                LoggingIn = false;
            }
        }

        private async void CacheLogin_Click(object sender, RoutedEventArgs e)
        {
            LoggingIn = true;
            try
            {
                await Task.Run(() => __CacheLogin(UserVkId));

                ErrorMessage = null;
                LoggedIn = true;
            }
            catch (AuthIDException)
            {
                ErrorMessage = "Error finding your page. Check page address.";
            }
            catch (AuthException)
            {
                ErrorMessage = "Error accessing vk.com. Try again later.";
            }
            finally
            {
                LoggingIn = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}
