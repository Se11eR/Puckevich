﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using PuckevichCore.DataVirtualization;
using PuckevichCore.Interfaces;
using PuckevichPlayer.Controls;
using PuckevichPlayer.Virtualizing;
using Page = System.Windows.Controls.Page;

namespace PuckevichPlayer
{
    /// <summary>
    /// Interaction logic for p_Player.xaml
    /// </summary>
    public partial class p_Player : Page, INotifyPropertyChanged
    {
        private const int PAGE_SIZE = 100;
        private const int PAGE_TIMEOUT = 1000 * 60; //1 minute

        private readonly IItemsProvider<IAudio> __VkProvider;
        private readonly IItemsProvider<IAudio> __CachedProvider;
        private IList<AudioModel> __List;
        private volatile AudioModel __CurrentActive;
        private readonly SemaphoreSlim __ProgressSemaphore = new SemaphoreSlim(1);

        public p_Player(IItemsProvider<IAudio> vkProvider, IItemsProvider<IAudio> cachedProvider, string userFirstName)
        {
            __VkProvider = vkProvider;
            __CachedProvider = cachedProvider;
            InitializeComponent();
            DataContext = this;
            AudioList = GetModelsList();

            PlayerTitle = String.Format("{0}'s music", userFirstName);
        }

        private IList<AudioModel> GetModelsList(bool cached = false)
        {
            return
                new AsyncVirtualizingCollection<AudioModel>(
                    new AudioModelProviderWrapper(cached ? __CachedProvider : __VkProvider),
                    PAGE_SIZE,
                    PAGE_TIMEOUT);
        }

        private async void AudioEntry_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            await __ProgressSemaphore.WaitAsync();
            try
            {
                if (!(sender is AudioEntry))
                    return;

                var audioModel = (sender as AudioEntry).DataContext as AudioModel;
                if (audioModel == null)
                    return;

                if (__CurrentActive == null)
                {
                    Interlocked.Exchange(ref __CurrentActive, audioModel);
                }
                else
                {
                    if (__CurrentActive != audioModel)
                    {
                        switch (__CurrentActive.AudioState)
                        {
                            case PlayingState.NotInit:
                                break;
                            case PlayingState.Stopped:
                                break;
                            case PlayingState.Paused:
                            case PlayingState.Playing:
                                __CurrentActive.Stop();
                                __CurrentActive = audioModel;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        switch (audioModel.AudioState)
                        {
                            case PlayingState.NotInit:
                            case PlayingState.Stopped:
                                __CurrentActive = null;
                                break;
                        }
                    }
                }

                audioModel.AudioEntryClicked();
            }
            finally
            {
                __ProgressSemaphore.Release();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public string PlayerTitle { get; private set; }

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
    }

}
