using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuckevichCore
{
    public interface IAudio
    {
        long SongId { get; }

        long UserId { get; }

        string Title { get; }

        string Artist { get; }

        int Duration { get; }

    }

    public delegate void AudioPlaybackFinishedHandler(IManagedPlayable audio);

    public interface IManagedPlayable : IAudio, IDisposable
    {

        void Init();

        void Play();

        void Pause();

        void Stop();

        int PercentagePlayed { get; }

        event AudioPlaybackFinishedHandler AudioPlaybackFinished;
    }
}
