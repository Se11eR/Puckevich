using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        /// Принимает дескриптор аудио-файла и возвращает поток для записи в него
        /// </summary>
        /// <param name="audio"></param>
        /// <returns></returns>
        Task<ICacheStream> CreateCacheStream(IAudio audio);

        /// <summary>
        /// Возвращает поток кешированного аудиофайла
        /// </summary>
        /// <param name="audioId"></param>
        /// <returns></returns>
        Task<ICacheStream> LookupCacheStream(long audioId);

        /// <summary>
        /// Удаляет кешированную аудиозапись
        /// </summary>
        /// <param name="auidiId"></param>
        void RemovecachedAudio(long auidiId);

        /// <summary>
        /// Возвращает статус состояния в хранилище для аудио-файла. 
        /// </summary>
        /// <param name="audioId">Vk audio id</param>
        /// <param name="bytesDownloaded">Если файл частично выкачан, это количество выкачанных байт, иначе - 0</param>
        /// <returns></returns>
        AudioStorageStatus GetStatus(long audioId);
    }
}
