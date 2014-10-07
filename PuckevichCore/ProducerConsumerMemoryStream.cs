using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PuckevichCore
{
    internal class ProducerConsumerMemoryStream : IDisposable
    {
        private readonly Stream __InnerStream;
        private readonly ICacheStream __CacheStream;
        private long __ReadPosition;
        private long __WritePosition;
        private readonly object __Lock;
        private bool __WriteFinished;
        private bool __IsFlushingNow;

        private readonly EventWaitHandle __WriteWaitHandle = new AutoResetEvent(false);

        public ProducerConsumerMemoryStream(ICacheStream cacheStream = null)
        {
            __InnerStream = new MemoryStream();
            if (cacheStream != null)
                __CacheStream = cacheStream;
            __Lock = new object();
        }

        public bool WriteFinished
        {
            get
            {
                return __WriteFinished;
            }
            set
            {
                lock (__Lock)
                {
                    __WriteFinished = value;
                    __WriteWaitHandle.Set();
                }
            }
        }

        public void Reset()
        {
            __WritePosition = 0;
            __ReadPosition = 0;
            __WriteFinished = false;
        }

        private void SetFlushing()
        {
            lock (__Lock)
            {
                __WriteWaitHandle.Set();
                __IsFlushingNow = true;
            }
        }

        public void CopyToInnerStream()
        {
            if (__CacheStream != null)
            {
                var initialPosition = __CacheStream.Position;
                var toWrite = __CacheStream.Position;
                __CacheStream.Position = 0;
                var buf = new byte[8192];

                int allRead = 0;
                while (allRead < toWrite)
                {
                    int read;
                    allRead += (read = __CacheStream.Read(buf, 0, buf.Length));
                    Write(buf, 0, read);
                }

                __CacheStream.Position = initialPosition;
            }
        }

        public async Task CopyToInnerStreamAsync()
        {
            if (__CacheStream != null)
            {
                await Task.Run((Action)CopyToInnerStream);
            }
        }

        public void FlushToCache(ICacheStream cacheStream)
        {
            if (cacheStream == null) throw new ArgumentNullException("cacheStream");

            SetFlushing();

            if (__WritePosition > cacheStream.Position)
            {
                var toWrite = __WritePosition - cacheStream.Position;
                var buf = new byte[16384];
                __InnerStream.Position = cacheStream.Position;

                int allRead = 0;
                while (allRead < toWrite)
                {
                    int read;
                    allRead += (read = __InnerStream.Read(buf, 0, buf.Length));
                    cacheStream.Write(buf, 0, read);
                }

                cacheStream.Flush();
            }
        }

        public async Task FlushToCacheAsync(ICacheStream cacheStream)
        {
            if (cacheStream == null) throw new ArgumentNullException("cacheStream");

            SetFlushing();

            if (__WritePosition > cacheStream.Position)
            {
                var toWrite = __WritePosition - cacheStream.Position;
                var buf = new byte[16384];
                __InnerStream.Position = cacheStream.Position;

                await Task.Run(() =>
                {
                    int allRead = 0;
                    while (allRead < toWrite)
                    {
                        int read;
                        allRead += (read = __InnerStream.Read(buf, 0, buf.Length));
                        cacheStream.Write(buf, 0, read);
                    }
                });

                await cacheStream.FlushAsync();
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int red;
            lock (__Lock)
            {
                if (__IsFlushingNow)
                    return 0;

                __InnerStream.Position = __ReadPosition;
                red = __InnerStream.Read(buffer, offset, count);
                __ReadPosition = __InnerStream.Position;
            }
            if (!WriteFinished && red == 0)
                __WriteWaitHandle.WaitOne();

            return red;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            lock (__Lock)
            {
                if (__IsFlushingNow)
                    return;

                __InnerStream.Position = __WritePosition;
                __InnerStream.Write(buffer, offset, count);
                __WritePosition = __InnerStream.Position;
            }
            __WriteWaitHandle.Set();
        }

        public long WritePosition
        {
            get { return __WritePosition; }
        }

        public void Dispose()
        {
            if (__InnerStream != null)
                __InnerStream.Close();
            //if (__CacheStream != null)
            //    __CacheStream.Dispose();

            __WriteWaitHandle.Dispose();
        }
    }
}
