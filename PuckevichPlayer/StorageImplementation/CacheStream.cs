using System;
using System.IO;
using System.Threading.Tasks;
using PuckevichCore;

namespace PuckevichPlayer.StorageImplementation
{
    internal class CacheStream : ICacheStream
    {
        private readonly Stream __Stream;
        private readonly Task<Task> __UpdateFileTask;
        private long? __FileLength;

        public CacheStream(Stream stream, Task<Task> updateFileTask)
        {
            __Stream = stream;
            __UpdateFileTask = updateFileTask;
            __FileLength = __Stream.Length;
        }

        public AudioStorageStatus Status
        {
            get
            {
                if (__Stream.Position >= __FileLength)
                    return AudioStorageStatus.Stored;
                if (__Stream.Position > 0)
                    return AudioStorageStatus.PartiallyStored;

                return AudioStorageStatus.NotStored;
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return __Stream.Read(buffer, offset, count);
        }

        public async Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            return await __Stream.ReadAsync(buffer, offset, count);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            __Stream.Write(buffer, offset, count);
        }

        public async Task WriteAsync(byte[] buffer, int offset, int count)
        {
            await __Stream.WriteAsync(buffer, offset, count);
        }

        public long? Length
        {
            get { return __FileLength; }
            set { __FileLength = value; }
        }

        public long Position
        {
            get { return __Stream.Position; }
            set
            {
                __Stream.Position = value;
            }
        }

        public async Task FlushAsync()
        {
            await __Stream.FlushAsync();
            Length = __Stream.Length;
            await __UpdateFileTask;
        }

        public void Dispose()
        {
            __Stream.Dispose();
        }
    }
}