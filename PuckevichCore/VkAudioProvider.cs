using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet;
using VkNet.Model.Attachments;

namespace PuckevichCore
{
    internal class VkAudioProvider : IItemsProvider<IAudio>
    {
        private readonly VkApi __Api;
        private readonly VkAudioFactory __Factory;

        public VkAudioProvider(VkApi api, VkAudioFactory factory)
        {
            __Api = api;
            __Factory = factory;
        }

        private IEnumerable<Audio> GetAudiosFromApi(int offset, int count)
        {
            var audios = __Api.Audio.Get(__Api.UserId.Value, null, null, count, offset);
            //Далее идет невероятный баг API вконтакте
            //иногда оно возвращает не то кол-ов записей, которое запросили (тестировал прямо на https://vk.com/dev/audio.get их родным тестером)
            while (audios.Count < count)
            {
                var difference = count - audios.Count;
                audios = __Api.Audio.Get(__Api.UserId.Value, null, null, count + difference, offset);
            }
            return audios;
        }

        public int FetchCount()
        {
            return __Api.Audio.GetCount(__Api.UserId.Value);
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
