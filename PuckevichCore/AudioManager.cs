using System;
using System.Collections.Generic;
using System.Threading;
using Un4seen.Bass;
using VkNet;
using VkNet.Enums.Filters;

namespace PuckevichCore
{
    public class AudioManager: IDisposable
    {
        private static readonly Object __SingletoneLock = new Object();
        private static AudioManager __Instance;

        private const int APP_ID = 4544915;
        private AudioInfoProvider __InfoProvider;
        internal readonly ISet<IManagedPlayable> OpenedChannels = new HashSet<IManagedPlayable>();

        private AudioManager()
        {
            
        }

        public static AudioManager Instance
        {
            get
            {
                if (__Instance != null) return __Instance;
                Monitor.Enter(__SingletoneLock);
                var temp = new AudioManager();
                Interlocked.Exchange(ref __Instance, temp);
                Monitor.Exit(__SingletoneLock);
                return __Instance;
            }
        }

        public void Init(string email, string password, IAudioStorage storage, IWebDownloader downloader)
        {
            var api = new VkApi();
            api.Authorize(APP_ID, email, password, Settings.Audio);
            __InfoProvider = new AudioInfoProvider(api, new AudioInfoFactory(storage, downloader));

            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                throw new Exception("Failed to initizlize BASS! Error code: " + Bass.BASS_ErrorGetCode());
            }
        }

        public IItemsProvider<IAudio> AudioInfoProvider
        {
            get { return __InfoProvider; }
        }

        public void Dispose()
        {
            foreach (var audio in OpenedChannels)
            {
                audio.Stop();
            }

            Bass.BASS_Free();
        }
    }
}
