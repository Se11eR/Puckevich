using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuckevichCore;

namespace PuckevichPlayer.Controls.SampleData
{
    internal class StubAudioModel
    {
        public string Title
        {
            get
            {
                return "Whiplash";
            }
        }

        public string Artist
        {
            get
            {
                return "Hank Levy";
            }
        }

        public PlayingState AudioState
        {
            get
            {
                return PlayingState.Paused;
            }
        }

        public int Duration
        {
            get
            {
                return 131;
            }
        }

        public int TimePlayed
        {
            get
            {
                return 31;
            }
            set
            {
                
            }
        }

        public double Downloaded
        {
            get
            {
                return 70.0;
            }
        }

        public bool IsCached
        {
            get
            {
                return true;
            }
        }
    }
}
