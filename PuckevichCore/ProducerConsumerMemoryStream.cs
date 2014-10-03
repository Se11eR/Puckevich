using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace PuckevichCore
{
    internal class ProducerConsumerMemoryStream : IDisposable
    {
        private readonly MemoryStream __InnerStream;
        private long __ReadPosition;
        private long __WritePosition;
        private readonly object __Lock;
        private bool __WriteFinished;
        private long __InitialStreamPosition;

        private readonly EventWaitHandle __WriteWaitHandle = new AutoResetEvent(false);

        public ProducerConsumerMemoryStream()
        {
            __InnerStream = new MemoryStream();
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

        public async Task Flush([NotNull] ICacheStream cacheStream)
        {
            if (cacheStream == null) throw new ArgumentNullException("cacheStream");

            lock (__Lock)
            {
                __WriteWaitHandle.Set();
                __InnerStream.Flush();
                
                __WritePosition = 0;
                __ReadPosition = 0;
                __WriteFinished = false;
            }

            if (__WritePosition > cacheStream.Position)
            {
                var buf = new byte[__WritePosition - cacheStream.Position];
                __InnerStream.Position = cacheStream.Position;

                int read = 0;
                while ((read = __InnerStream.Read(buf, 0, buf.Length)) < (__WritePosition - cacheStream.Position))
                {
                    await cacheStream.WriteAsync(buf, 0, read);
                }

                await cacheStream.FlushAsync();
            }
        }

        public long Length
        {
            get
            {
                lock (__Lock)
                {
                    return __InnerStream.Length;
                }
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int red;
            lock (__Lock)
            {
                __InnerStream.Position = __ReadPosition;
                red = __InnerStream.Read(buffer, offset, count);
                __ReadPosition = __InnerStream.Position;
            }
            if (!WriteFinished && red == 0)
                __WriteWaitHandle.WaitOne();

            return red;
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            lock (__Lock)
            {
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
            __InnerStream.Close();
        }
    }
}
