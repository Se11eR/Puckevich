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

        bool CheckCached(IAudio audio);

        /// <summary>
        /// Удаляет кешированную аудиозапись
        /// </summary>
        /// <param name="auidiId"></param>
        void RemovecachedAudio(long auidiId);
    }
}
