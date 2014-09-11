using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet;

namespace PuckevichCore
{
    public class VkAudioList : IReadOnlyList<IManagedPlayable>
    {
        private readonly VkApi __Api;
        public const int MIN_CACHED_AUDIOS = 20;
        private readonly List<VkAudio> __InternaList = new List<VkAudio>(MIN_CACHED_AUDIOS);
        private int __VkAudioCount = -1;

        private readonly VkAudioEnumerator __Enumerator;

        public VkAudioList(VkApi api, IAudioStorage storage)
        {
            __Api = api;
            SetCountFromApi();
            __Enumerator = new VkAudioEnumerator(__InternaList, __VkAudioCount, __Api, storage, MIN_CACHED_AUDIOS);
        }

        private void SetCountFromApi()
        {
            __VkAudioCount = __Api.Audio.GetCount(__Api.UserId.Value);
        }

        public IEnumerator<IManagedPlayable> GetEnumerator()
        {
            return __Enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return __VkAudioCount; }
        }

        public IManagedPlayable this[int index]
        {
            get
            {
                while (index >= __InternaList.Count)
                {
                    __Enumerator.MoveNext();
                }

                return __InternaList[index];
            }
        }
    }
}
