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
        private const int APP_ID = 4544915;
        private readonly IAudioStorage __Storage;
        private readonly VkApi __Api;
        private readonly VkAudioList __List;

        public VkAudioManager(string email, string password, IAudioStorage storage)
        {
            __Storage = storage;
            __Api = new VkApi();
            __Api.Authorize(APP_ID, email, password, Settings.Audio);
            __List = new VkAudioList(__Api, storage);

            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                throw new Exception("Failed to initizlize BASS! Error code: " + Bass.BASS_ErrorGetCode());
            }
        }

        public VkAudioList AudioList
        {
            get { return __List; }
        }

        public void Dispose()
        {
            Bass.BASS_Free();
        }
    }
}
