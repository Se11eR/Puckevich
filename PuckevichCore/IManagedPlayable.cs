using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuckevichCore
{
    public enum PlayingState
    {
        NotInit,
        Stopped,
        Paused,
        Playing
    }


    public interface IAudio
    {
        long AudioId { get; }

        long UserId { get; }

        string Title { get; }

        string Artist { get; }

        int Duration { get; }

        IManagedPlayable Playable { get; }
    }

    public delegate void AudioStoppedEvent(IManagedPlayable audio);

    public delegate void AudioStalledEvent(IManagedPlayable audio);

    public interface IManagedPlayable
    {

        void Play();

        void Pause();

        void Stop();

        int SecondsPlayed { get; }

        double PercentageDownloaded { get; }

        event AudioStoppedEvent AudioStopped;

        PlayingState State { get; }

        event AudioStalledEvent AudioStalled;
    }
}
