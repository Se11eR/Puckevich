using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace PuckevichCore
{

    internal class PlayableAudio : IManagedPlayable
    {
        private readonly BASS_FILEPROCS __BASSFileProcs;
        private readonly SYNCPROC __EndStreamProc;

        private readonly long __Id;
        private readonly IAudioStorage __Storage;
        private readonly EventWaitHandle __WebHandle = new AutoResetEvent(false);
        private readonly IWebDownloader __Downloader;
        private readonly Uri __Url;
        private AudioStorageStatus __StorageStatus;
        private IStoredAudioContainer __Container;
        private ProducerConsumerMemoryStream __CacheStream;
        private PlayingState __PlayingState = PlayingState.NotInit;
        private int __BassStream;
        private long __LengthInBytes;
        private double __PercentsDownloaded;
        private readonly Stopwatch __PlayingStopwatch = new Stopwatch();
        private long __BytesDownloaded;

        internal PlayableAudio(IAudioStorage storage, IWebDownloader downloader, long id, Uri url)
        {
            __Storage = storage;
            __Downloader = downloader;
            __Id = id;
            __Url = url;
            __StorageStatus = __Storage.GetStatus(__Id);

            __BASSFileProcs = new BASS_FILEPROCS((IntPtr user) => { },
                                                 (IntPtr user) => 0,
                                                 ProducerConsumerReadProc,
                                                 (long offset, IntPtr user) => false);

            __EndStreamProc = (int handle, int channel, int data, IntPtr user) => OnAudioStopped();
        }

        private void WebDownloader()
        {
            long audioLengthInBytes;

            if (__PlayingState == PlayingState.Stopped)
                return;

            var webStream = __Downloader.GetUrlStream(__Url, out audioLengthInBytes);
            __LengthInBytes = audioLengthInBytes;
            __BytesDownloaded = __CacheStream.Length;

            if (webStream == null)
                throw new ApplicationException("Error while creating Url Stream!");

            const int BUFFER_SIZE = 100000; //100 kB
            var buffer = new byte[BUFFER_SIZE];

            int blockRead = 0;
            int lengthRead;

            if (__PlayingState == PlayingState.Stopped)
                return;

            while ((lengthRead = webStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                blockRead += lengthRead;
                __BytesDownloaded += lengthRead;
                __CacheStream.Write(buffer, 0, lengthRead);

                if (blockRead >= BUFFER_SIZE)
                {
                    blockRead = 0;
                    __WebHandle.Set();
                }

                __PercentsDownloaded = (double)__BytesDownloaded / __LengthInBytes;

                if (__PlayingState == PlayingState.Stopped)
                    return;
            }

            __CacheStream.WriteFinished = true;
        }

        private int ProducerConsumerReadProc(IntPtr buffer, int length, IntPtr user)
        {
            try
            {
                var readbuffer = new byte[length];
                var todo = length;
                if (!__CacheStream.WriteFinished)
                    __WebHandle.WaitOne();

                while (todo > 0)
                {
                    int lengthRead = __CacheStream.Read(readbuffer, 0, todo);
                    if (lengthRead > 0)
                        Marshal.Copy(readbuffer, 0, buffer, lengthRead);
                    else if (lengthRead == 0 && __CacheStream.WriteFinished)
                        //Тогда поток автоматически "остановится", вызовется сооветствующий хендлер
                        return 0;

                    todo -= lengthRead;
                }
                return length;
            }
            catch
            {
                return 0;
            }
        }

        private void HandleStoredFile()
        {
            __Container = __Storage.GetAudio(__Id);
            __CacheStream = new ProducerConsumerMemoryStream(__Container.CachedStream);
            __CacheStream.WriteFinished = true;
            __LengthInBytes = __CacheStream.Length;
        }

        private void HandleNotStoredFile(bool partiallyStored)
        {
            if (partiallyStored)
            {
                __Container = __Storage.GetAudio(__Id);
                __CacheStream = new ProducerConsumerMemoryStream(__Container.CachedStream);
            }
            else
            {
                __Container = __Storage.StoreAudio(__Id);
                __CacheStream = new ProducerConsumerMemoryStream(__Container.CachedStream);
            }

            //Качаем песню с vk
            Task.Factory.StartNew(WebDownloader);
        }

        internal void Init()
        {
            switch (__StorageStatus)
            {
                case AudioStorageStatus.Stored:
                    HandleStoredFile();
                    break;
                case AudioStorageStatus.PartiallyStored:
                case AudioStorageStatus.NotStored:
                    HandleNotStoredFile(__StorageStatus == AudioStorageStatus.PartiallyStored);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            __BassStream = Bass.BASS_StreamCreateFileUser(BASSStreamSystem.STREAMFILE_BUFFER,
                                                          BASSFlag.BASS_DEFAULT,
                                                          __BASSFileProcs,
                                                          IntPtr.Zero);
            if (__BassStream == 0)
                Error.HandleBASSError("BASS_StreamCreateFileUser");
            Bass.BASS_ChannelSetSync(__BassStream, BASSSync.BASS_SYNC_END, 0, __EndStreamProc, IntPtr.Zero);
        }
        private void OnAudioStopped()
        {
            //Do the stuff when stopped (called Stop() or stream fininshed)
            //---
            switch (__StorageStatus)
            {
                case AudioStorageStatus.Stored:

                    break;
                case AudioStorageStatus.PartiallyStored:
                    __CacheStream.Flush();
                    if (__BytesDownloaded >= __LengthInBytes)
                        __StorageStatus = AudioStorageStatus.Stored;

                    break;
                case AudioStorageStatus.NotStored:
                    if (__BytesDownloaded > 0)
                    {
                        __CacheStream.Flush();
                        __StorageStatus = __BytesDownloaded < __LengthInBytes
                                              ? AudioStorageStatus.PartiallyStored
                                              : AudioStorageStatus.Stored;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            __PlayingStopwatch.Reset();
            __WebHandle.Reset();
            //---
            var handler = AudioStopped;
            if (handler != null)
                handler(this);
        }

        public void Play()
        {
            __PlayingState = PlayingState.Playing;
            if (!Bass.BASS_ChannelPlay(__BassStream, false))
                Error.HandleBASSError("BASS_ChannelPlay");
            __PlayingStopwatch.Start();
        }

        public void Pause()
        {
            __PlayingState = PlayingState.Paused;
            if (!Bass.BASS_ChannelPause(__BassStream))
                Error.HandleBASSError("BASS_ChannelPause");
            __PlayingStopwatch.Stop();
        }

        public void Stop()
        {
            __PlayingState = PlayingState.Stopped;
            if (!Bass.BASS_ChannelStop(__BassStream))
                Error.HandleBASSError("BASS_ChannelStop");
            OnAudioStopped();
        }

        public event AudioStoppedEvent AudioStopped;

        public double PercentagePlayed
        {
            get { return __PercentsDownloaded; }
        }

        public double PercentageDownloaded
        {
            get { return __PercentsDownloaded; }
        }

        ~PlayableAudio()
        {
            if (__PlayingState != PlayingState.NotInit && __PlayingState != PlayingState.Stopped)
                Stop();
            Bass.BASS_StreamFree(__BassStream);
            __WebHandle.Dispose();
            __CacheStream.Dispose();
        }

        public event AudioStalledEvent AudioStalled;

        public int SecondsPlayed
        {
            get
            {
                return __PlayingStopwatch.Elapsed.Seconds;
            }
        }

        public PlayingState State
        {
            get
            {
                return __PlayingState;
            }
        }
    }
}
