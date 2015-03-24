using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using PuckevichCore.Exceptions;
using PuckevichCore.Interfaces;
using Un4seen.Bass;

namespace PuckevichCore
{

    internal class AudioPlayable
    {
        private const int WEB_BUFFER_SIZE = 1024 * 16;
        private const int WEB_THRESHOLD = 1024 * 128;

        private readonly BASS_FILEPROCS __BassFileProcs;
        private readonly SYNCPROC __EndStreamProc;
        private Task __WebDownloaderTask;

        private readonly IAudio __Audio;
        private readonly IAudioStorage __Storage;
        private readonly IWebDownloader __Downloader;
        private readonly Uri __Url;
        private ICacheStream __CacheStream;
        private ProducerConsumerMemoryStream __ProducerConsumerStream;
        private int __BassStream;
        private readonly StopWatchWithOffset __PlayingStopwatch = new StopWatchWithOffset();

        private double __DownloadedFracion;
        private long __BytesReadToBass = 0;
        private volatile bool __ThresholdDownloaded;
        private volatile bool __TasksInitialized;
        private volatile bool __RequestTasksStop;

        private readonly object __SeekLock = new object();

        public event Action DownloadedFracionChanged;
        public event Action AudioNaturallyEnded;

        internal AudioPlayable(IAudio audio, IAudioStorage storage, IWebDownloader downloader, Uri url)
        {
            __Audio = audio;
            __Storage = storage;
            __Downloader = downloader;
            __Url = url;

            __BassFileProcs = new BASS_FILEPROCS(user => { },
                                                 user => 
                                                     __CacheStream.AudioSize,
                                                 BassReadProc,
                                                 (offset, user) => true);

            __EndStreamProc = (handle, channel, data, user) =>
            {
                __RequestTasksStop = true;
                CleanActions();
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

        private void CleanActions()
        {
            __PlayingStopwatch.Reset();
            __ProducerConsumerStream.Dispose();
            __ProducerConsumerStream = null;
        }

        private void WaitAndReset()
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
            __ThresholdDownloaded = false;
            __BytesReadToBass = 0;
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
                    throw new PuckevichException("Error while creating Url Stream!");

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

                    if (blockRead >= WEB_THRESHOLD)
                    {
                        __ThresholdDownloaded = true;
                    }

                    if (__CacheStream == null)
                        return;

                    if (__RequestTasksStop)
                        return;
                }


            }
            finally
            {
                if (webStream != null)
                    webStream.Dispose();
                __ProducerConsumerStream.FlushToCache();
                __ProducerConsumerStream.WriteFinished = true;
                __ThresholdDownloaded = true;
            }
        }

        private int BassReadProc(IntPtr buffer, int length, IntPtr user)
        {
            if (__RequestTasksStop)
                return 0;

            if (__ProducerConsumerStream == null)
            {
                while (__ProducerConsumerStream == null && !__RequestTasksStop)
                    Thread.Sleep(1);
            }

            if (__ProducerConsumerStream == null)
                throw new PuckevichException("BassReadProc: __ProducerConsumerStream == null");

            if (__RequestTasksStop)
                return 0;

            try
            {
                var toRead = length;
                var readbuffer = new byte[toRead];
                var read = 0;
                while (read < toRead)
                {
                    if (__RequestTasksStop)
                        return 0;

                    int lengthRead = __ProducerConsumerStream.Read(readbuffer, 0, toRead - read);
                    if (lengthRead > 0)
                        Marshal.Copy(readbuffer, 0, buffer + read, lengthRead);
                    else if (lengthRead == 0 && __ProducerConsumerStream.WriteFinished)
                        //Тогда поток автоматически "остановится" и вызовется сооветствующий хендлер
                        return 0;
                    else
                        Thread.Sleep(1);

                    read += lengthRead;
                }

                return toRead;
            }
            catch
            {
                return 0;
            }
            finally
            {
                __BytesReadToBass += length;
                DownloadedFracion = (double)__BytesReadToBass / __CacheStream.AudioSize;
            }
        }

        public void Init()
        {
            __CacheStream = __Storage.GetCacheStream(__Audio);

            switch (__CacheStream.Status)
            {
                case AudioStorageStatus.Stored:
                    __ProducerConsumerStream = new ProducerConsumerMemoryStream(__CacheStream);
                    __ProducerConsumerStream.LoadToMemory();
                    __ProducerConsumerStream.WriteFinished = true;
                    DownloadedFracion = 1.0;

                    break;
                case AudioStorageStatus.PartiallyStored:
                case AudioStorageStatus.NotStored:
                    if (__Url == null)
                        throw new PuckevichException("__Url cannot be null for not cached audio!");

                    __WebDownloaderTask = Task.Run((Action)WebDownloader);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            __TasksInitialized = true;

            while (__CacheStream.Status != AudioStorageStatus.Stored && !__ThresholdDownloaded)
                Thread.Sleep(1);

            if (__RequestTasksStop)
                return;

            __BassStream = Bass.BASS_StreamCreateFileUser(BASSStreamSystem.STREAMFILE_BUFFER,
                                                          BASSFlag.BASS_DEFAULT,
                                                          __BassFileProcs,
                                                          IntPtr.Zero);

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

        public void Seek(double second)
        {
            if (!Monitor.TryEnter(__SeekLock)) 
                return;

            try
            {
                var seekByte = Bass.BASS_ChannelSeconds2Bytes(__BassStream, second);

                if (!Bass.BASS_ChannelSetPosition(__BassStream, seekByte, BASSMode.BASS_POS_BYTES))
                    Error.HandleBASSError("BASS_ChannelSetPosition");

                __PlayingStopwatch.Restart();
                __PlayingStopwatch.Elapsed = TimeSpan.FromSeconds(second);
            }
            finally
            {
                Monitor.Exit(__SeekLock);
            }
        }

        private void StreamStopTasksWait()
        {
            __RequestTasksStop = true;
            if (!Bass.BASS_ChannelStop(__BassStream))
                Error.HandleBASSError("BASS_ChannelStop");
            Bass.BASS_StreamFree(__BassStream);

            WaitAndReset();
        }

        public void Stop()
        {
            StreamStopTasksWait();
            CleanActions();
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

        public double SecondsPlayed
        {
            get { return Math.Round(__PlayingStopwatch.Elapsed.TotalSeconds); }
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
            Bass.BASS_StreamFree(__BassStream);

            if (__ProducerConsumerStream != null)
                __ProducerConsumerStream.Dispose();
        }
    }
}
