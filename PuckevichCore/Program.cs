using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.Filters;

namespace PuckevichCore
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            int appID = 4544915;
            string email = "";
            string pass = "";
            Settings scope = Settings.Audio;

            var manager = new VkAudioManager(email, pass, new Storage());
            var s = manager.AudioList[1];
            s.Init();
            s.Play();
        }
    }

    public class Storage : IAudioStorage
    {

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public AudioStorageStatus GetStatus(long audioId, out long bytesDownloaded)
        {
            throw new NotImplementedException();
        }


        IStoredAudioContainer IAudioStorage.StoreAudio(long audioId)
        {
            throw new NotImplementedException();
        }

        IStoredAudioContainer IAudioStorage.GetAudio(long audioId)
        {
            throw new NotImplementedException();
        }
    }
}
