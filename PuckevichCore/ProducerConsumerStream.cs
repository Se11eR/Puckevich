using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuckevichCore
{
    internal class ProducerConsumerStream : Stream
    {
        private readonly MemoryStream __InnerStream;
        private long __ReadPosition;
        private long __WritePosition;
        private readonly object __Lock;

        public ProducerConsumerStream()
        {
            __InnerStream = new MemoryStream();
            __Lock = new object();
        }

        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return true; } }

        public override void Flush()
        {
            lock (__InnerStream)
            {
                __InnerStream.Flush();
            }
        }

        public override long Length
        {
            get
            {
                lock (__InnerStream)
                {
                    return __InnerStream.Length;
                }
            }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (__Lock)
            {
                __InnerStream.Position = __ReadPosition;
                int red = __InnerStream.Read(buffer, offset, count);
                __ReadPosition = __InnerStream.Position;

                return red;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (__Lock)
            {
                __InnerStream.Position = __WritePosition;
                __InnerStream.Write(buffer, offset, count);
                __WritePosition = __InnerStream.Position;
            }
        }
    }
}
