﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PuckevichCore.Interfaces;

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

        public ProducerConsumerMemoryStream(ICacheStream cacheStream)
        {
            if (cacheStream == null)
                throw new ArgumentNullException("cacheStream");

            __InnerStream = new MemoryStream();
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
                }
            }
        }

        public void Reset()
        {
            __WritePosition = 0;
            __ReadPosition = 0;
            __WriteFinished = false;
        }

        private void SetFlushing(bool value)
        {
            lock (__Lock)
            {
                __IsFlushingNow = value;
            }
        }

        public void LoadToMemory()
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

        public void FlushToCache()
        {
            SetFlushing(true);

            if (__WritePosition > __CacheStream.Position)
            {
                var toWrite = __WritePosition - __CacheStream.Position;
                var buf = new byte[16384];
                __InnerStream.Position = __CacheStream.Position;

                int allRead = 0;
                while (allRead < toWrite)
                {
                    int read;
                    allRead += (read = __InnerStream.Read(buf, 0, buf.Length));
                    __CacheStream.Write(buf, 0, read);
                }

                __CacheStream.Flush();
            }

            SetFlushing(false);
        }
        public int Read(byte[] buffer, int offset, int count)
        {
            int read;
            lock (__Lock)
            {
                if (__IsFlushingNow)
                    return 0;

                __InnerStream.Position = __ReadPosition;
                read = __InnerStream.Read(buffer, offset, count);
                __ReadPosition = __InnerStream.Position;
            }

            return read;
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
        }

        public long WritePosition
        {
            get
            {
                lock (__Lock)
                    return __WritePosition;
            }
        }

        public long ReadPosition
        {
            get
            {
                lock (__Lock)
                    return __ReadPosition;
            }
            set
            {
                lock (__Lock)
                    __ReadPosition = value;
            }
        }

        public void Dispose()
        {
            if (__InnerStream != null)
                __InnerStream.Dispose();
            if (__CacheStream != null)
                __CacheStream.Dispose();
        }
    }
}
