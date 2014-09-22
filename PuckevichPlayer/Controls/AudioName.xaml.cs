using System;
using System.Collections.Generic;
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

namespace PuckevichPlayer.Controls
{
    /// <summary>
    /// Interaction logic for AudioName.xaml
    /// </summary>
    public partial class AudioName : UserControl
    {
        public static readonly DependencyProperty ArtistProperty = DependencyProperty.Register("Artist",
                                                                                               typeof(string),
                                                                                               typeof(AudioName),
                                                                                               new PropertyMetadata("",
                                                                                                                    DepProperyChanged));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title",
                                                                                              typeof(string),
                                                                                              typeof(AudioName),
                                                                                              new PropertyMetadata("",
                                                                                                                   DepProperyChanged));

        public static readonly DependencyProperty MaxNameLengthProperty = DependencyProperty.Register("MaxNameLength",
                                                                                                      typeof(int),
                                                                                                      typeof(AudioName),
                                                                                                      new PropertyMetadata(-1,
                                                                                                                           DepProperyChanged));

        private const string LINE = " ‒ ";

        public AudioName()
        {
            InitializeComponent();
        }

        private static void DepProperyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (AudioName)d;
            if (obj.MaxNameLength == -1)
            {
                obj.TitleTextBlock.Text = LINE + obj.Title;
                return;
            }

            var fullname = obj.Artist + LINE + obj.Title;
            if (fullname.Length > obj.MaxNameLength)
            {
                if (fullname.Length - obj.Title.Length > obj.MaxNameLength)
                {
                    if (fullname.Length - obj.Title.Length - LINE.Length - 1 > obj.MaxNameLength)
                    {
                        obj.ArtistTextBlock.Text = obj.Artist.Substring(0, obj.MaxNameLength - 3) + "...";
                        obj.TitleTextBlock.Text = "";
                    }
                    else
                    {
                        obj.ArtistTextBlock.Text = obj.Artist + "...";
                        obj.TitleTextBlock.Text = "";
                    }
                }
                else
                {
                    obj.ArtistTextBlock.Text = obj.Artist;
                    if ((obj.MaxNameLength - (fullname.Length - obj.Title.Length)) > 3)
                        obj.TitleTextBlock.Text = LINE + obj.Title.Substring(0, obj.MaxNameLength - obj.Artist.Length - 3) + "...";
                    else
                        obj.TitleTextBlock.Text = LINE + "...";
                }
            }
            else
            {
                obj.ArtistTextBlock.Text = obj.Artist;
                obj.TitleTextBlock.Text = LINE + obj.Title;
            }
        }

        public int MaxNameLength
        {
            get
            {
                return (int)GetValue(MaxNameLengthProperty);
            }
            set
            {
                SetValue(MaxNameLengthProperty, value);
            }
        }

        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        public string Artist
        {
            get
            {
                return (string)GetValue(ArtistProperty);
            }
            set
            {
                SetValue(ArtistProperty, value);
            }
        }

        public string FullName
        {
            get
            {
                return Title + LINE + Artist;
            }
        }
    }
}
