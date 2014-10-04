using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuckevichCore
{
    public interface ICacheStream: IDisposable
    {
        int Read(byte[] buffer, int offset, int count);

        void Write(byte[] buffer, int offset, int count);

        long? Length { get; set; }

        long Position
        {
            get;
            set;
        }

        Task FlushAsync();

        AudioStorageStatus Status
        {
            get;
        }
    }
}
