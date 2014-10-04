﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using PuckevichCore;
using PuckevichPlayer.Controls;
using Page = System.Windows.Controls.Page;

namespace PuckevichPlayer
{
    /// <summary>
    /// Interaction logic for p_Player.xaml
    /// </summary>
    public partial class p_Player : Page, INotifyPropertyChanged
    {
        private IList<AudioModel> __List;

        public p_Player(IList<AudioModel> list)
        {
            InitializeComponent();
            DataContext = this;
            AudioList = list;
        }

        public IList<AudioModel> AudioList
        {
            get
            {
                return __List;
            }
            set
            {
                __List = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void AudioEntry_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is AudioEntry))
                return;

            var audioModel= (sender as AudioEntry).DataContext as AudioModel;
            if (audioModel == null)
                return;

            await audioModel.AudioEntryClicked();
        }

        private async void P_Player_OnUnloaded(object sender, RoutedEventArgs e)
        {
            foreach (var audioModel in __List)
            {
                switch (audioModel.AudioState)
                {
                    case PlayingState.NotInit:
                        break;
                    case PlayingState.Stopped:
                        break;
                    case PlayingState.Paused:
                        await audioModel.Stop();
                        break;
                    case PlayingState.Playing:
                        await audioModel.Stop();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
