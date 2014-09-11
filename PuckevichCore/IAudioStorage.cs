using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuckevichCore
{
    public interface IAudioStorage
    {
        void Initialize();

        /// <summary>
        /// Принимает url для аудио-файла и возвращает поток кешированного
        /// </summary>
        /// <param name="song"></param>
        /// <param name="audioId"></param>
        /// <returns></returns>
        Stream StoreAudio(Uri song, long audioId);

        /// <summary>
        /// Возвращает поток кешированного аудиофайла
        /// </summary>
        /// <param name="audioId"></param>
        /// <returns></returns>
        Stream GetAudio(long audioId);


        bool IsStored(long audioId);
    }
}
