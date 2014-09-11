using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet;
using VkNet.Model.Attachments;

namespace PuckevichCore
{
    internal class VkAudioEnumerator: IEnumerator<VkAudio>
    {
        private readonly VkApi __Api;
        private readonly IAudioStorage __Storage;
        private readonly List<VkAudio> __InternalList;
        private int __CacheSize;
        private readonly int __Count;

        private int __Current = -1;
        internal VkAudioEnumerator(List<VkAudio> internalList, int count, VkApi api, IAudioStorage storage, int cacheSize)
        {
            __Api = api;
            __Storage = storage;
            __CacheSize = cacheSize;
            __InternalList = internalList;
            __Count = count;
        }

        private IEnumerable<Audio> GetAudiosFromApi(int offset, int count)
        {
            return __Api.Audio.Get(__Api.UserId.Value, null, null, count, offset);
        }

        private static void AppendFromApiList(VkApi api, IAudioStorage storage, List<VkAudio> internalList, IEnumerable<Audio> vkaudio)
        {
            internalList.AddRange(
                                  vkaudio.Select(audio =>
                                  {
                                      var b = new UriBuilder("http", audio.Url.Host, 80, audio.Url.AbsolutePath);
                                      return new VkAudio(storage,
                                                         audio.Id,
                                                         api.UserId.Value,
                                                         audio.Title,
                                                         audio.Artist,
                                                         audio.Duration,
                                                         b.Uri);

                                  }));
        }

        public int CacheSize
        {
            get { return __CacheSize; }
            set
            {
                __CacheSize = value;
            }
        }

        public VkAudio Current
        {
            get
            {
                if (__Current == -1 || __Current > __Count)
                    throw new InvalidOperationException();

                return __InternalList[__Current];
            }
        }

        public void Dispose()
        {
            __InternalList.Clear();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if (__Current >= __Count)
                return false;

            __Current++;

            if (__Current >= __Count)
                return false;

            if (__Current >= __InternalList.Count)
            {
                AppendFromApiList(__Api, __Storage, __InternalList, GetAudiosFromApi(__Current, CacheSize));
            }

            return true;
        }

        public void Reset()
        {
            __Current = -1;
        }
    }
}
