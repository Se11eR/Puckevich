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
            s.Pause();
            s.Play();
            s.Play();
            s.Pause();
            s.Play();
            s.Pause();
            s.Stop();
        }
    }

    public class Storage : IAudioStorage
    {

        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public Stream GetAudio(long audioId)
        {
            return null;
        }

        public Stream StoreAudio(Uri song, long audioId)
        {
            var client = new WebClient();
            return client.OpenRead(song);
        }

        public bool IsStored(long audioId)
        {
            return false;
        }

        
    }
}
