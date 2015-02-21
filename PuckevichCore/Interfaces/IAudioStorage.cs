using System;
using System.Threading.Tasks;

namespace PuckevichCore
{
    public enum AudioStorageStatus
    {
        Stored,
        PartiallyStored,
        NotStored
    }

    public interface IAudioStorage : IDisposable
    {
        void Initialize();

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
