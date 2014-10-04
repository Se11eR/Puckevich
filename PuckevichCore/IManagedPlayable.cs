using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuckevichCore
{
    public delegate void PlayingStateChangedEvent(IManagedPlayable sender);

    public interface IManagedPlayable : IDisposable
    {

        void Play();

        void Pause();

        Task Stop();

        Task Init();

        int SecondsPlayed { get; }

        double Downloaded { get; }

        event PlayingStateChangedEvent PlayingStateChanged;

        PlayingState State { get; }
    }
}
