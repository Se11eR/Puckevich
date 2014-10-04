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

        private readonly IAudio __Audio;
        private readonly IAudioStorage __Storage;
        private readonly EventWaitHandle __WebHandle = new AutoResetEvent(false);
        private readonly IWebDownloader __Downloader;
        private readonly Uri __Url;
        private ICacheStream __CacheStream;
        private ProducerConsumerMemoryStream __ProducerConsumerStream;
        private PlayingState __PlayingState = PlayingState.NotInit;
        private int __BassStream;
        private double __PercentsDownloaded;
        private readonly Stopwatch __PlayingStopwatch = new Stopwatch();

        internal PlayableAudio(IAudio audio, IAudioStorage storage, IWebDownloader downloader, Uri url)
        {
            __Audio = audio;
            __Storage = storage;
            __Downloader = downloader;
            __Url = url;

            __BASSFileProcs = new BASS_FILEPROCS((IntPtr user) => { },
                                                 (IntPtr user) => 0,
                                                 ProducerConsumerReadProc,
                                                 (long offset, IntPtr user) => false);

            __EndStreamProc = (int handle, int channel, int data, IntPtr user) =>
            { var _ = WhenStoppedAsync(); };
        }

        private PlayingState PlayingState
        {
            get
            {
                return __PlayingState;
            }
            set
            {
                var oldState = __PlayingState;
                __PlayingState = value;

                if (oldState != __PlayingState)
                {
                    var handler = PlayingStateChanged;
                    if (handler != null)
                        handler(this);
                }
            }
        }

        private async Task WhenStoppedAsync()
        {
            //Do the stuff when stopped (called StopAsync() or stream fininshed)
            //---
            switch (__CacheStream.Status)
            {
                case AudioStorageStatus.Stored:

                    break;
                case AudioStorageStatus.PartiallyStored:
                case AudioStorageStatus.NotStored:
                    await __ProducerConsumerStream.FlushToCacheAsync(__CacheStream);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            __PlayingStopwatch.Reset();
            __WebHandle.Reset();

            //---
        }

        private void WhenStopped()
        {
            //Do the stuff when stopped (called StopAsync() or stream fininshed)
            //---
            switch (__CacheStream.Status)
            {
                case AudioStorageStatus.Stored:

                    break;
                case AudioStorageStatus.PartiallyStored:
                case AudioStorageStatus.NotStored:
                    __ProducerConsumerStream.FlushToCache(__CacheStream);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            __PlayingStopwatch.Reset();
            __WebHandle.Reset();

            //---
        }

        private void WebDownloader()
        {
            long audioLengthInBytes;

            if (PlayingState == PlayingState.Stopped)
                return;

            var webStream = __Downloader.GetUrlStream(__Url, out audioLengthInBytes);

            if (__CacheStream.Length.HasValue)
            {
                if (__CacheStream.Length.Value != audioLengthInBytes)
                {
                    //Hm...
                }
            }
            else
            {
                __CacheStream.Length = audioLengthInBytes;
            }

            __ProducerConsumerStream = new ProducerConsumerMemoryStream();

            if (webStream == null)
                throw new ApplicationException("Error while creating Url Stream!");

            const int BUFFER_SIZE = 100000; //100 kB
            var buffer = new byte[BUFFER_SIZE];

            int blockRead = 0;
            int lengthRead;

            if (PlayingState == PlayingState.Stopped)
                return;

            while ((lengthRead = webStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                blockRead += lengthRead;
                __ProducerConsumerStream.Write(buffer, 0, lengthRead);

                if (blockRead >= BUFFER_SIZE)
                {
                    blockRead = 0;
                    __WebHandle.Set();
                }

                __PercentsDownloaded = (double)__ProducerConsumerStream.WritePosition / __CacheStream.Length.Value;

                if (PlayingState == PlayingState.Stopped)
                    return;
            }

            __ProducerConsumerStream.WriteFinished = true;
        }

        private int ProducerConsumerReadProc(IntPtr buffer, int length, IntPtr user)
        {
            try
            {
                var readbuffer = new byte[length];
                var todo = length;

                while (__ProducerConsumerStream == null)
                    Thread.Sleep(1);

                if (!__ProducerConsumerStream.WriteFinished)
                    __WebHandle.WaitOne();

                while (todo > 0)
                {
                    int lengthRead = __ProducerConsumerStream.Read(readbuffer, 0, todo);
                    if (lengthRead > 0)
                        Marshal.Copy(readbuffer, 0, buffer, lengthRead);
                    else if (lengthRead == 0 && __ProducerConsumerStream.WriteFinished)
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

        public void Init()
        {
            __CacheStream = __Storage.GetCacheStream(__Audio);

            switch (__CacheStream.Status)
            {
                case AudioStorageStatus.Stored:
                    __ProducerConsumerStream = new ProducerConsumerMemoryStream();
                    __ProducerConsumerStream.WriteFinished = true;

                    break;
                case AudioStorageStatus.PartiallyStored:
                case AudioStorageStatus.NotStored:
                    Task.Factory.StartNew(WebDownloader);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task InitAsync()
        {
            __CacheStream = await __Storage.GetCacheStreamAsync(__Audio);

            switch (__CacheStream.Status)
            {
                case AudioStorageStatus.Stored:
                    __ProducerConsumerStream = new ProducerConsumerMemoryStream();
                    __ProducerConsumerStream.WriteFinished = true;

                    break;
                case AudioStorageStatus.PartiallyStored:
                case AudioStorageStatus.NotStored:
                    var t = Task.Factory.StartNew(WebDownloader);

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

        public void Play()
        {
            PlayingState = PlayingState.Playing;
            if (!Bass.BASS_ChannelPlay(__BassStream, false))
                Error.HandleBASSError("BASS_ChannelPlay");
            __PlayingStopwatch.Start();
        }

        public void Pause()
        {
            PlayingState = PlayingState.Paused;
            if (!Bass.BASS_ChannelPause(__BassStream))
                Error.HandleBASSError("BASS_ChannelPause");
            __PlayingStopwatch.Stop();
        }

        public void Stop()
        {
            StopInternal();
            WhenStopped();
            
        }

        public async Task StopAsync()
        {
            StopInternal();
            await WhenStoppedAsync();
        }

        private void StopInternal()
        {
            PlayingState = PlayingState.Stopped;
            if (!Bass.BASS_ChannelStop(__BassStream))
                Error.HandleBASSError("BASS_ChannelStop");
        }

        public event PlayingStateChangedEvent PlayingStateChanged;

        public double Downloaded
        {
            get { return __PercentsDownloaded; }
        }

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
                return PlayingState;
            }
        }

        ~PlayableAudio()
        {
            if (__PlayingState != PlayingState.Stopped && __PlayingState != PlayingState.NotInit)
            {
                throw new ApplicationException("This PlayableAudio was not stoppped before destruction!");
            }

            Bass.BASS_StreamFree(__BassStream);

            if (__WebHandle != null)
                __WebHandle.Dispose();
            if (__ProducerConsumerStream != null)
                __ProducerConsumerStream.Dispose();
            if(__CacheStream != null)
                __CacheStream.Dispose();
        }
        
    }
}
