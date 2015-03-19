using System;
using System.Threading.Tasks;

namespace PuckevichCore.Interfaces
{
    public enum AudioStorageStatus
    {
        Stored,
        PartiallyStored,
        NotStored
    }

    internal interface IAudioStorage : IDisposable
    {
        /// <summary>
        /// Возвращает поток кешированного аудиофайла, или создает новый, если файла нет в хранилище.
        /// </summary>
        /// <param name="audio"></param>
        /// <returns></returns>
        Task<ICacheStream> GetCacheStreamAsync(IAudio audio);

        ICacheStream GetCacheStream(IAudio audio);

        IAudio GetAt(int index);

        bool CheckCached(IAudio audio);

        void RemovecachedAudio(long auidiId);

        int Count { get; }
    }
}
