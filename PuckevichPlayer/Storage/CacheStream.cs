using System;
using System.IO;
using System.Threading.Tasks;
using PuckevichCore;

namespace PuckevichPlayer.Storage
{
    internal class CacheStream : ICacheStream
    {
        private readonly Stream __Stream;
        private readonly JsonAudioModel __Model;
        private long __AudioSize;

        internal CacheStream(Stream stream, JsonAudioModel model)
        {
            __Stream = stream;
            __Model = model;
        }

        public AudioStorageStatus Status
        {
            get
            {
                if (__AudioSize > 0)
                {
                    if (__Stream.Position >= __AudioSize)
                        return AudioStorageStatus.Stored;
                    if (__Stream.Position > 0)
                        return AudioStorageStatus.PartiallyStored;
                }

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

        public long AudioSize
        {
            get { return __AudioSize; }
            set { __AudioSize = value; }
        }

        public long Position
        {
            get { return __Stream.Position; }
            set
            {
                __Stream.Position = value;
            }
        }

        public void Flush()
        {
            __Stream.Flush();
            __Model.AudioSize = AudioSize;
        }

        public async Task FlushAsync()
        {
            await __Stream.FlushAsync();
            __Model.AudioSize = AudioSize;
        }

        public void Dispose()
        {
            __Stream.Dispose();
        }
    }
}