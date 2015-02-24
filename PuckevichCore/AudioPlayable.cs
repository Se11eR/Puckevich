using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PuckevichCore.Interfaces;
using Un4seen.Bass;

namespace PuckevichCore
{

    internal class AudioPlayable
    {
        private const int WEB_BUFFER_SIZE = 1024 * 16;
        private const int READ_PROC_START_THRESHOLD = 1024 * 32;

        private readonly BASS_FILEPROCS __BASSFileProcs;
        private readonly SYNCPROC __EndStreamProc;

        private readonly IAudio __Audio;
        private readonly IAudioStorage __Storage;
        private readonly EventWaitHandle __WebHandle = new ManualResetEvent(false);
        private Task __WebDownloaderTask;
        private readonly IWebDownloader __Downloader;
        private readonly Uri __Url;
        private ICacheStream __CacheStream;
        private ProducerConsumerMemoryStream __ProducerConsumerStream;
        private int __BassStream;
        private double __DownloadedFracion;
        private readonly Stopwatch __PlayingStopwatch = new Stopwatch();

        private bool __TasksInitialized = false;
        private volatile bool __RequestTasksStop = false;

        public event Action DownloadedFracionChanged;
        public event Action AudioNaturallyEnded;

        internal AudioPlayable(IAudio audio, IAudioStorage storage, IWebDownloader downloader, Uri url)
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
            {
                __RequestTasksStop = true;
                FlushAndCleanAfterStop();
                OnAudioNaturallyEnded();
            };

            if (storage.CheckCached(audio))
            {
                DownloadedFracion = 1.0;
            }
        }

        private void OnDownloadedFracionChanged()
        {
            var handler = DownloadedFracionChanged;
            if (handler != null)
                handler();
        }

        private void OnAudioNaturallyEnded()
        {
            var handler = AudioNaturallyEnded;
            if (handler != null)
                handler();
        }

        private async Task FlushAndCleanAfterStopAsync()
        {
            switch (__CacheStream.Status)
            {
                case AudioStorageStatus.Stored:

                    break;
                case AudioStorageStatus.PartiallyStored:
                case AudioStorageStatus.NotStored:
                    await __ProducerConsumerStream.FlushToCacheAsync().ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CleanActions();
        }

        private void FlushAndCleanAfterStop()
        {
            switch (__CacheStream.Status)
            {
                case AudioStorageStatus.Stored:

                    break;
                case AudioStorageStatus.PartiallyStored:
                case AudioStorageStatus.NotStored:
                    __ProducerConsumerStream.FlushToCache();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CleanActions();
        }

        private void CleanActions()
        {
            __PlayingStopwatch.Reset();
            __WebHandle.Set();
            __WebHandle.Reset();
            __ProducerConsumerStream.Dispose();
            __ProducerConsumerStream = null;
        }

        private void WaitTasksStop()
        {
            try
            {
                Task.WaitAll(__WebDownloaderTask);
                __WebDownloaderTask.Dispose();
            }
            catch (Exception)
            {
            }
            __RequestTasksStop = false;
            __TasksInitialized = false;
        }

        private void WebDownloader()
        {
            Stream webStream = null;
            try
            {
                long audioLengthInBytes;

                if (__RequestTasksStop)
                    return;

                webStream = __Downloader.GetUrlStream(__Url, __CacheStream.Position, out audioLengthInBytes);

                if (webStream == null)
                    throw new ApplicationException("Error while creating Url Stream!");

                if (__RequestTasksStop)
                    return;

                if (__CacheStream.Status == AudioStorageStatus.NotStored)
                    __CacheStream.AudioSize = audioLengthInBytes;

                if (__RequestTasksStop)
                    return;

                __ProducerConsumerStream = new ProducerConsumerMemoryStream(__CacheStream);

                __ProducerConsumerStream.LoadToMemory();

                if (__RequestTasksStop)
                    return;

                var buffer = new byte[WEB_BUFFER_SIZE];

                int blockRead = 0;
                int lengthRead;

                if (__RequestTasksStop)
                    return;

                while ((lengthRead = webStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    blockRead += lengthRead;
                    __ProducerConsumerStream.Write(buffer, 0, lengthRead);

                    if (__RequestTasksStop)
                        return;

                    if (blockRead >= READ_PROC_START_THRESHOLD)
                    {
                        blockRead = 0;
                        __WebHandle.Set();
                    }

                    if (__CacheStream == null)
                        return;

                    DownloadedFracion = (double)__ProducerConsumerStream.WritePosition / __CacheStream.AudioSize;

                    if (__RequestTasksStop)
                        return;
                }
            }
            finally
            {
                if (webStream != null)
                    webStream.Dispose();
                __ProducerConsumerStream.WriteFinished = true;
                __WebHandle.Set();
            }
        }

        private int ProducerConsumerReadProc(IntPtr buffer, int length, IntPtr user)
        {
            if (__ProducerConsumerStream == null || !__ProducerConsumerStream.WriteFinished)
                __WebHandle.WaitOne();

            if (__RequestTasksStop)
                return 0;

            __WebHandle.Reset();
            if (__ProducerConsumerStream == null)
                throw new ApplicationException("ProducerConsumerReadProc: __ProducerConsumerStream == null");

            if (__RequestTasksStop)
                return 0;

            try
            {
                var readbuffer = new byte[length];
                var toRead = length;

                while (toRead > 0)
                {
                    if (__RequestTasksStop)
                        return 0;

                    int lengthRead = __ProducerConsumerStream.Read(readbuffer, 0, toRead);
                    if (lengthRead > 0)
                        Marshal.Copy(readbuffer, 0, buffer, lengthRead);
                    else if (lengthRead == 0 && __ProducerConsumerStream.WriteFinished)
                        //Тогда поток автоматически "остановится" и вызовется сооветствующий хендлер
                        return 0;

                    toRead -= lengthRead;
                }
                return length;
            }
            catch
            {
                return 0;
            }
        }

        public async Task InitAsync()
        {
            __CacheStream = await __Storage.GetCacheStreamAsync(__Audio).ConfigureAwait(false);

            switch (__CacheStream.Status)
            {
                case AudioStorageStatus.Stored:
                    __ProducerConsumerStream = new ProducerConsumerMemoryStream(__CacheStream);
                    await __ProducerConsumerStream.LoadToMemoryAsync();
                    __ProducerConsumerStream.WriteFinished = true;
                    DownloadedFracion = 1.0;

                    break;
                case AudioStorageStatus.PartiallyStored:
                case AudioStorageStatus.NotStored:
                    __WebDownloaderTask = Task.Run((Action)WebDownloader);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            __TasksInitialized = true;
            await
                Task.Run(
                         () =>
                         __BassStream =
                         Bass.BASS_StreamCreateFileUser(BASSStreamSystem.STREAMFILE_BUFFER,
                                                        BASSFlag.BASS_DEFAULT,
                                                        __BASSFileProcs,
                                                        IntPtr.Zero));

            WhenInit();
        }

        private void WhenInit()
        {
            if (__BassStream == 0)
                Error.HandleBASSError("BASS_StreamCreateFileUser");
            Bass.BASS_ChannelSetSync(__BassStream,
                                     BASSSync.BASS_SYNC_END,
                                     0,
                                     __EndStreamProc,
                                     IntPtr.Zero);
        }

        public void Play()
        {
            if (!Bass.BASS_ChannelPlay(__BassStream, false))
                Error.HandleBASSError("BASS_ChannelPlay");
            __PlayingStopwatch.Start();
        }

        public void Pause()
        {
            if (!Bass.BASS_ChannelPause(__BassStream))
                Error.HandleBASSError("BASS_ChannelPause");
            __PlayingStopwatch.Stop();
        }

        private void StreamStopTasksWait()
        {
            __RequestTasksStop = true;
            if (!Bass.BASS_ChannelStop(__BassStream))
                Error.HandleBASSError("BASS_ChannelStop");
            Bass.BASS_StreamFree(__BassStream);

            WaitTasksStop();
        }

        public void Stop()
        {
            StreamStopTasksWait();
            FlushAndCleanAfterStop();
        }

        public async Task StopAsync()
        {
            StreamStopTasksWait();
            await FlushAndCleanAfterStopAsync();
        }

        public double DownloadedFracion
        {
            get { return __DownloadedFracion; }
            set
            {
                __DownloadedFracion = value;
                OnDownloadedFracionChanged();
            }
        }

        public int SecondsPlayed
        {
            get
            {
                return (int)Math.Round(__PlayingStopwatch.Elapsed.TotalSeconds);
            }
        }

        public bool TasksInitialized
        {
            get
            {
                return __TasksInitialized;
            }
        }

        ~AudioPlayable()
        {
            //if (__PlayingState != PlayingState.Stopped && __PlayingState != PlayingState.NotInit)
            //{
            //    throw new ApplicationException("This AudioPlayable was not stoppped before destruction!");
            //}

            Bass.BASS_StreamFree(__BassStream);

            if (__WebHandle != null)
            {
                __WebHandle.Set();
                __WebHandle.Dispose();
            }
            if (__ProducerConsumerStream != null)
                __ProducerConsumerStream.Dispose();
        }
    }
}
