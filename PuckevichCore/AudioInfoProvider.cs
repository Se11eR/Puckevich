using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PuckevichCore.DataVirtualization;
using PuckevichCore.Interfaces;
using VkNet;
using VkNet.Model.Attachments;

namespace PuckevichCore
{
    internal class AudioInfoProvider : IItemsProvider<IAudio>
    {
        private const int QUERY_TIME_THRESHOLD = 333;

        private static readonly Stopwatch __QueryWatch = new Stopwatch();
        private static object __Lock = new object();
        private static int __WholeCount;


        private readonly Func<long, int, int, ReadOnlyCollection<Audio>> __GetAudio;
        private readonly Func<long, int> __GetCount;
        private readonly AudioInfoFactory __InfoFactory;
        private readonly PlayingStateChangedEventHandler __OptionalStateChangedHandler;
        private readonly long __UserID;

        public AudioInfoProvider(Func<long, int, int, ReadOnlyCollection<Audio>> getAudio,
            Func<long, int> getCount,
            AudioInfoFactory infoFactory,
            long userID,
            PlayingStateChangedEventHandler optionalStateChangedHandler = null)
        {
            __GetAudio = getAudio;
            __GetCount = getCount;
            __InfoFactory = infoFactory;
            __UserID = userID;
            __OptionalStateChangedHandler = optionalStateChangedHandler;
        }

        private IEnumerable<VkNet.Model.Attachments.Audio> GetAudiosFromApi(int offset, int count)
        {
            lock (__Lock)
            {
                if (__QueryWatch.ElapsedMilliseconds < QUERY_TIME_THRESHOLD)
                {
                    Thread.Sleep(QUERY_TIME_THRESHOLD);
                }
                __QueryWatch.Restart();

                var audios = __GetAudio(__UserID, count, offset);
                //Далее идет невероятный баг API вконтакте
                //иногда оно возвращает не то кол-ов записей, которое запросили 
                //(тестировал прямо на https://vk.com/dev/audio.get их родным тестером)
                while (audios.Count < count)
                {
                    var difference = count - audios.Count;
                    var newCount = count + difference;
                    if (offset + newCount > __WholeCount)
                    {
                        newCount = __WholeCount - offset;
                    }
                    audios = __GetAudio(__UserID, newCount, offset);
                    Thread.Sleep(QUERY_TIME_THRESHOLD);
                }
                return audios;
            }
        }

        public int FetchCount()
        {
            return __WholeCount = __GetCount(__UserID);
        }

        public IList<IAudio> FetchRange(int startIndex, int count)
        {
            var list = new List<IAudio>(count);
            var vkAudios = GetAudiosFromApi(startIndex, count);
            list.AddRange(vkAudios.Select(audio =>
            {
                var b = new UriBuilder("http", audio.Url.Host, 80, audio.Url.AbsolutePath);
                return __InfoFactory.Create(audio.Id,
                    __UserID,
                    audio.Title,
                    audio.Artist,
                    audio.Duration,
                    b.Uri,
                    __OptionalStateChangedHandler);
            }));

            return list;
        }
    }
}
