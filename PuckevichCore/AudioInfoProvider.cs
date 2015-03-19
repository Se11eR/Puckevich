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
        private static readonly object __Lock = new object();
        private static int __WholeCount;

        private readonly Func<long, int, int, ReadOnlyCollection<Audio>> __GetAudio;
        private readonly Func<long, int> __GetCount;
        private readonly AudioInfoFactory __InfoFactory;
        private readonly PlayingStateChangedEventHandler __OptionalStateChangedHandler;
        private readonly long __UserId;

        public AudioInfoProvider(Func<long, int, int, ReadOnlyCollection<Audio>> getAudio,
            Func<long, int> getCount,
            AudioInfoFactory infoFactory,
            long userId,
            PlayingStateChangedEventHandler optionalStateChangedHandler = null)
        {
            __GetAudio = getAudio;
            __GetCount = getCount;
            __InfoFactory = infoFactory;
            __UserId = userId;
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

                var audios = __GetAudio(__UserId, count, offset);
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
                    audios = __GetAudio(__UserId, newCount, offset);
                    Thread.Sleep(QUERY_TIME_THRESHOLD);
                }
                return audios;
            }
        }

        public int FetchCount()
        {
            return __WholeCount = __GetCount(__UserId);
        }

        public IList<IAudio> FetchRange(int startIndex, int count)
        {
            var list = new List<IAudio>(count);
            var vkAudios = GetAudiosFromApi(startIndex, count).ToList();
            for (int i = 0; i < vkAudios.Count; i++)
            {
                var audio = vkAudios[i];
                var b = new UriBuilder("http", audio.Url.Host, 80, audio.Url.AbsolutePath);
                list.Add(__InfoFactory.Create(__UserId,
                    startIndex + i,
                    audio,
                    b.Uri,
                    __OptionalStateChangedHandler));
            }
            
            return list;
        }
    }
}
