using System;
using System.Collections.Generic;
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

        private readonly VkApi __Api;
        private readonly AudioInfoFactory __InfoFactory;
        private readonly PlayingStateChangedEventHandler __OptionalStateChangedHandler;

        public AudioInfoProvider(VkApi api,
                                 AudioInfoFactory infoFactory,
                                 PlayingStateChangedEventHandler optionalStateChangedHandler = null)
        {
            __Api = api;
            __InfoFactory = infoFactory;
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
            
                var audios = __Api.Audio.Get(__Api.UserId.Value, null, null, count, offset);
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
                    audios = __Api.Audio.Get(__Api.UserId.Value, null, null, newCount, offset);
                    Thread.Sleep(QUERY_TIME_THRESHOLD);
                }
                return audios;
            }
        }

        public int FetchCount()
        {
            return __WholeCount = __Api.Audio.GetCount(__Api.UserId.Value);
        }

        public IList<IAudio> FetchRange(int startIndex, int count)
        {
            var list = new List<IAudio>(count);
            var vkAudios = GetAudiosFromApi(startIndex, count);
            list.AddRange(vkAudios.Select(audio =>
            {
                var b = new UriBuilder("http", audio.Url.Host, 80, audio.Url.AbsolutePath);
                return __InfoFactory.Create(audio.Id,
                                            __Api.UserId.Value,
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
