using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using VkNet;
using VkNet.Model.Attachments;

namespace PuckevichCore
{
    internal class VkAudioProvider : IItemsProvider<IAudio>
    {
        private readonly VkApi __Api;
        private readonly VkAudioFactory __Factory;

        private const int __QueryTimeThreshold = 333;
        private static readonly Stopwatch __QueryWatch = new Stopwatch();
        private static object __Lock = new object();
        private static int __WholeCount;

        public VkAudioProvider(VkApi api, VkAudioFactory factory)
        {
            __Api = api;
            __Factory = factory;
        }

        private IEnumerable<Audio> GetAudiosFromApi(int offset, int count)
        {
            lock (__Lock)
            {
                if (__QueryWatch.ElapsedMilliseconds < 333)
                {
                    Thread.Sleep(__QueryTimeThreshold);
                }
                __QueryWatch.Restart();
            
                var audios = __Api.Audio.Get(__Api.UserId.Value, null, null, count, offset);
                //Далее идет невероятный баг API вконтакте
                //иногда оно возвращает не то кол-ов записей, которое запросили (тестировал прямо на https://vk.com/dev/audio.get их родным тестером)
                while (audios.Count < count)
                {
                    var difference = count - audios.Count;
                    var newCount = count + difference;
                    if (offset + newCount > __WholeCount)
                    {
                        newCount = __WholeCount - offset;
                    }
                    audios = __Api.Audio.Get(__Api.UserId.Value, null, null, newCount, offset);
                    Thread.Sleep(__QueryTimeThreshold);
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
                return __Factory.Create(audio.Id, __Api.UserId.Value, audio.Title, audio.Artist, audio.Duration, b.Uri);
            }));

            return list;
        }
    }
}
