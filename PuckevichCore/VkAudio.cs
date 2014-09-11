using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace PuckevichCore
{
    internal enum VkAudioState
    {
        Stopped,
        Paused,
        Playing
    }

    public class VkAudio : IManagedPlayable
    {
        private readonly IAudioStorage __Storage;
        private readonly long __AudioId;
        private readonly long __UserId;
        private readonly string __Title;
        private readonly string __Artist;
        private readonly int __Duration;
        private readonly Uri __Url;

        private bool __IsInitialized;
        private int __BASS_Stream;
        private VkAudioState __State = VkAudioState.Stopped;
        private bool __Restart = false;
        private readonly object __BASS_OperationsLock = new object();

        internal VkAudio(IAudioStorage storage, long audioId, long userId, string title, string artist, int duration, Uri url)
        {
            __Storage = storage;
            __AudioId = audioId;
            __UserId = userId;
            __Title = title;
            __Artist = artist;
            __Duration = duration;
            __Url = url;

            if (storage.IsStored(__AudioId))
            {
                __IsInitialized = true;
            }
            else
            {
                __IsInitialized = false;
            }
        }

        public void Init()
        {
            if (!__IsInitialized)
            {
                __BASS_Stream = Bass.BASS_StreamCreateURL(__Url.OriginalString, 0, BASSFlag.BASS_DEFAULT, null, IntPtr.Zero);
                if (__BASS_Stream == 0)
                    throw new Exception("Error creating BASS sctream. Error code: " + Bass.BASS_ErrorGetCode());

                __IsInitialized = true;
            }
        }


        public long SongId
        {
            get { return __AudioId; }
        }

        public long UserId
        {
            get { return __UserId; }
        }

        public string Title
        {
            get { return __Title; }
        }

        public string Artist
        {
            get { return __Artist; }
        }

        public int Duration
        {
            get { return __Duration; }
        }

        public void Play()
        {
            if (__IsInitialized)
            {
                lock (__BASS_OperationsLock)
                {
                    if (__State != VkAudioState.Playing)
                    {
                        if (!Bass.BASS_ChannelPlay(__BASS_Stream, __Restart))
                        {
                            var error = Bass.BASS_ErrorGetCode();
                            if (error == BASSError.BASS_ERROR_START)
                            {
                                Bass.BASS_Start();
                            }

                            throw new Exception("Error VkAudio.Play(). BASS error code: " + error.ToString());
                        }
                        __State = VkAudioState.Playing;
                        __Restart = false;
                    }
                }
            }
        }

        public void Pause()
        {
            if (__IsInitialized)
            {
                lock (__BASS_OperationsLock)
                {
                    if (__State == VkAudioState.Playing)
                    {
                        if (!Bass.BASS_ChannelPause(__BASS_Stream))
                        {
                            var error = Bass.BASS_ErrorGetCode();
                            if (error == BASSError.BASS_ERROR_ALREADY)
                            {
                                throw new Exception("BASS_ERROR_ALREADY in Pause! Probably double-pausing.");
                            }

                            throw new Exception("Error VkAudio.Pause(). BASS error code: " + error.ToString());
                        }
                        __State = VkAudioState.Paused;
                        __Restart = false;
                    }
                }
            }
        }

        public void Stop()
        {
            if (__IsInitialized)
            {
                lock (__BASS_OperationsLock)
                {
                    if (__State == VkAudioState.Playing)
                    {
                        if (!Bass.BASS_ChannelPause(__BASS_Stream))
                        {
                            var error = Bass.BASS_ErrorGetCode();
                            if (error == BASSError.BASS_ERROR_ALREADY)
                            {
                                throw new Exception("BASS_ERROR_ALREADY in Pause! Probably double-pausing.");
                            }

                            throw new Exception("Error VkAudio.Stop(). BASS error code: " + error.ToString());
                        }
                    }

                    __State = VkAudioState.Stopped;
                    __Restart = true;
                }
            }
        }

        public int PercentagePlayed
        {
            get { throw new NotImplementedException(); }
        }

        private void OnAudioPlaybackFinished()
        {
            var handler = AudioPlaybackFinished;
            if (handler != null)
                handler(this);
        }

        public event AudioPlaybackFinishedHandler AudioPlaybackFinished;

        public void Dispose()
        {
            Bass.BASS_StreamFree(__BASS_Stream);
        }
    }
}
