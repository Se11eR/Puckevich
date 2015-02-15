using System;
using System.Threading.Tasks;

namespace PuckevichCore
{
    public interface ICacheStream: IDisposable
    {
        int Read(byte[] buffer, int offset, int count);

        Task<int> ReadAsync(byte[] buffer, int offset, int count);

        void Write(byte[] buffer, int offset, int count);

        Task WriteAsync(byte[] buffer, int offset, int count);

        long AudioSize { get; set; }

        long Position
        {
            get;
            set;
        }

        /// <summary>
        /// Сохраняет данные потока в постоянную память. 
        /// </summary>
        void Flush();
        Task FlushAsync();

        AudioStorageStatus Status
        {
            get;
        }
    }
}
