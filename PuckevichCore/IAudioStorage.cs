﻿using System;
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
        /// Возвращает поток кешированного аудиофайла, или создает новый, если файла нет в хранилище.
        /// </summary>
        /// <param name="audioId"></param>
        /// <returns></returns>
        Task<ICacheStream> GetCacheStream(IAudio audio);

        /// <summary>
        /// Удаляет кешированную аудиозапись
        /// </summary>
        /// <param name="auidiId"></param>
        Task RemovecachedAudio(long auidiId);
    }
}
