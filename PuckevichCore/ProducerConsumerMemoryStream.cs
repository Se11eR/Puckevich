using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PuckevichCore
{
    internal class ProducerConsumerMemoryStream : Stream
    {
        private readonly MemoryStream __InnerStream;
        private long __ReadPosition;
        private long __WritePosition;
        private readonly object __Lock;
        private bool __WriteFinished;
        private readonly Stream __InitialStream;
        private long __InitialStreamPosition;

        private readonly EventWaitHandle __WriteWaitHandle = new AutoResetEvent(false);

        public ProducerConsumerMemoryStream()
        {
            __InnerStream = new MemoryStream();
            __Lock = new object();
        }

        public ProducerConsumerMemoryStream(Stream initialStream)
            :this()
        {
            __InitialStream = initialStream;
            __InitialStreamPosition = initialStream.Length;
            var byf = new byte[initialStream.Length];
            initialStream.Read(byf, 0, byf.Length);

            if (__InitialStreamPosition > 0)
                Write(byf, 0, byf.Length);
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

        #region Stream members

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override void Flush()
        {
            lock (__Lock)
            {
                __WriteWaitHandle.Set();
                __InnerStream.Flush();

                if (__InitialStream != null)
                {
                    var buf = new byte[__WritePosition - __InitialStreamPosition];
                    __InnerStream.Position = __InitialStreamPosition;
                    __InnerStream.Read(buf, 0, buf.Length);

                    __InitialStream.Write(buf, 0, buf.Length);
                    __InitialStreamPosition = __InitialStream.Length;
                }
                
                __WritePosition = 0;
                __ReadPosition = 0;
                __WriteFinished = false;
            }
        }

        public override long Length
        {
            get
            {
                lock (__Lock)
                {
                    return __InnerStream.Length;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
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

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (__Lock)
            {
                __InnerStream.Position = __WritePosition;
                __InnerStream.Write(buffer, offset, count);
                __WritePosition = __InnerStream.Position;
            }
            __WriteWaitHandle.Set();
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
