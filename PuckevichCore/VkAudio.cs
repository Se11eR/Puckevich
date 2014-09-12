using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
        private VkAudioState __State = VkAudioState.Stopped;
        private bool __Restart = false;
        private readonly object __BASS_OperationsLock = new object();

        private int __BASS_Stream;
        private readonly DOWNLOADPROC __DownloadDelegate;
        private Stream __CacheStream;
        private IStoredAudioContainer __Container;
        private byte[] __DownnloadHandlerByteBuffer;
        private AudioStorageStatus __StorageStatus;

        internal VkAudio(IAudioStorage storage, long audioId, long userId, string title, string artist, int duration, Uri url)
        {
            __Storage = storage;
            __AudioId = audioId;
            __UserId = userId;
            __Title = title;
            __Artist = artist;
            __Duration = duration;
            __Url = url;

            __DownloadDelegate = DownloadHandler;

            long bytesDownloaded;
            __StorageStatus = __Storage.GetStatus(__AudioId, out bytesDownloaded);
        }

        private void DownloadHandler(IntPtr buffer, int length, IntPtr user)
        {
            if (__CacheStream == null)
                return;

            if (buffer == IntPtr.Zero)
            {
                // finished downloading
                return;
            }

            if (__DownnloadHandlerByteBuffer == null || __DownnloadHandlerByteBuffer.Length < length)
                __DownnloadHandlerByteBuffer = new byte[length];

            Marshal.Copy(buffer, __DownnloadHandlerByteBuffer, 0, length);
            __CacheStream.Write(__DownnloadHandlerByteBuffer, 0, length);
        }

        private void CachedStreamPutter()
        {
            const int BUFFER_SIZE = 10000; //10KB
            var buffer = new byte[BUFFER_SIZE];

            int lengthRead;
            while ((lengthRead = __CacheStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                Bass.BASS_StreamPutData(__BASS_Stream, buffer, lengthRead);
            }
        }

        public void Init()
        {
            if (!__IsInitialized)
            {
                switch (__StorageStatus)
                {
                    case AudioStorageStatus.Stored:
                        __Container = __Storage.GetAudio(__AudioId);

                        __BASS_Stream = Bass.BASS_StreamCreatePush(__Container.Frequency, __Container.ChannelNumber, BASSFlag.BASS_DEFAULT, IntPtr.Zero);

                        //пишем в BASS канал
                        Task.Factory.StartNew(CachedStreamPutter);
                        break;
                    case AudioStorageStatus.PartiallyStored:

                        throw new NotImplementedException();

                        break;
                    case AudioStorageStatus.NotStored:
                        __Container = __Storage.StoreAudio(__AudioId);
                        __CacheStream = __Container.CachedStream;

                        __BASS_Stream = Bass.BASS_StreamCreateURL(__Url.OriginalString, 0, BASSFlag.BASS_DEFAULT, __DownloadDelegate, IntPtr.Zero);
                        if (__BASS_Stream == 0)
                            Error.HandleBASSError("BASS_StreamCreateURL");

                        var info = new BASS_CHANNELINFO();
                        if (!Bass.BASS_ChannelGetInfo(__BASS_Stream, info))
                            Error.HandleBASSError("BASS_ChannelGetInfo");

                        __Container.Frequency = info.freq;
                        __Container.ChannelNumber = info.chans;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                

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
