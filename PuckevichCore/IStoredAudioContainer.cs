using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuckevichCore
{
    public interface IStoredAudioContainer
    {
        Stream CachedStream { get; }

        int Frequency { get; set; }

        int ChannelNumber { get; set; }
    }
}
