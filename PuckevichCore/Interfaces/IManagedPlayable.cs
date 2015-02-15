﻿using System.Threading.Tasks;

namespace PuckevichCore
{
    public delegate void PlayingStateChangedEvent(IManagedPlayable sender);

    public interface IManagedPlayable
    {

        void Play();

        void Pause();

        void Stop();

        Task StopAsync();

        void Init();

        Task InitAsync();

        int SecondsPlayed { get; }

        double Downloaded { get; }

        event PlayingStateChangedEvent PlayingStateChanged;

        PlayingState State { get; }
    }
}