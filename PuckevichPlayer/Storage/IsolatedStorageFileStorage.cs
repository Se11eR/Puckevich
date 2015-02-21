using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuckevichCore.Interfaces;

namespace PuckevichPlayer.Storage
{
    internal class IsolatedStorageFileStorage : IFileStorage
    {
        private IsolatedStorageFile __IsoStorage;
        private const string ISOLATED_STORE_ROOT_DIR = "m_RootDir";

        private static FileInfo GetFileInfo(string path, IsolatedStorageFile store)
        {
            return new FileInfo(GetFullyQualifiedFileName(path, store));
        }

        private static string GetFullyQualifiedFileName(string path, IsolatedStorageFile store)
        {
            var fieldInfo = store.GetType()
                                 .GetField(ISOLATED_STORE_ROOT_DIR,
                                           System.Reflection.BindingFlags.NonPublic |
                                           System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
            {
                return Path.Combine(fieldInfo.GetValue(store).ToString(), path);
            }

            return "";
        }

        public IsolatedStorageFileStorage()
        {
            __IsoStorage =
                IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly,
                                             null,
                                             null);
        }

        public Stream CreateFile(string filepath)
        {
            return __IsoStorage.CreateFile(filepath);
        }

        public Stream OpenFile(string filepath, FileMode mode)
        {
            return __IsoStorage.OpenFile(filepath, mode);
        }

        public void RemoveFile(string filepath)
        {
            __IsoStorage.DeleteFile(filepath);
        }

        public bool FileExists(string filepath)
        {
            return __IsoStorage.FileExists(filepath);
        }

        public long GetFileSize(string filepath)
        {
            return GetFileInfo(filepath, __IsoStorage).Length;
        }

        public void Dispose()
        {
            __IsoStorage.Dispose();
        }
    }
}
