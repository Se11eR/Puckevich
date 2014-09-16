using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuckevichCore
{
    public interface IAudio: IManagedPlayable
    {
        long SongId { get; }

        long UserId { get; }

        string Title { get; }

        string Artist { get; }

        int Duration { get; }
    }

    public delegate void AudioStoppedEvent(IManagedPlayable audio);

    public delegate void AudioStalledEvent(IManagedPlayable audio);

    public interface IManagedPlayable: IDisposable
    {

        void Play();

        void Pause();

        void Stop();

        double PercentagePlayed { get; }

        int SecondsPlayed { get; }

        double PercentageDownloaded { get; }

        event AudioStoppedEvent AudioStopped;

        event AudioStalledEvent AudioStalled;
    }
}
