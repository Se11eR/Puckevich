using System;
using System.IO;

namespace PuckevichCore.Interfaces
{
    public interface IFileStorage : IDisposable
    {
        Stream CreateFile(string filepath);

        Stream OpenFile(string filepath, FileMode mode);

        void RemoveFile(string filepath);

        bool FileExists(string filepath);

        long GetFileSize(string filepath);
    }
}
