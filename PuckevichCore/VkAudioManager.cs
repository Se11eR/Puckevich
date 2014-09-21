using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using VkNet;
using VkNet.Enums.Filters;

namespace PuckevichCore
{
    public class VkAudioManager: IDisposable
    {
        private const int PAGE_SIZE = 30;
        private const int PAGE_TIMEOUT = 1000 * 60; //1 minute
        private const int APP_ID = 4544915;
        private readonly VirtualizingCollection<IAudio> __List;

        public VkAudioManager(string email, string password, IAudioStorage storage, IWebDownloader downloader)
        {
            var api = new VkApi();
            api.Authorize(APP_ID, email, password, Settings.Audio);
            __List = new AsyncVirtualizingCollection<IAudio>(new VkAudioProvider(api, new VkAudioFactory(storage, downloader)),
                                                        PAGE_SIZE,
                                                        PAGE_TIMEOUT);

            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                throw new Exception("Failed to initizlize BASS! Error code: " + Bass.BASS_ErrorGetCode());
            }
        }

        public IList<IAudio> AudioList
        {
            get { return __List; }
        }

        public void Dispose()
        {
            Bass.BASS_Free();
        }
    }
}
