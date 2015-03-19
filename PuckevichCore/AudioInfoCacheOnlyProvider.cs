using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuckevichCore.DataVirtualization;
using PuckevichCore.Interfaces;

namespace PuckevichCore
{
    internal class AudioInfoCacheOnlyProvider : IItemsProvider<IAudio>
    {
        private readonly AudioInfoFactory __InfoFactory;
        private readonly IAudioStorage __Cache;
        private readonly long __UserId;
        private readonly PlayingStateChangedEventHandler __OptionalStateChangedHandler;

        public AudioInfoCacheOnlyProvider(AudioInfoFactory infoFactory,
            IAudioStorage cache,
            long userId,
            PlayingStateChangedEventHandler optionalStateChangedHandler = null)
        {
            __InfoFactory = infoFactory;
            __Cache = cache;
            __UserId = userId;
            __OptionalStateChangedHandler = optionalStateChangedHandler;
        }

        public int FetchCount()
        {
            return __Cache.Count;
        }

        public IList<IAudio> FetchRange(int startIndex, int count)
        {
            return Enumerable.Range(startIndex, count).Select(i =>
            {
                IAudio stub = __Cache.GetAt(i);
                return __InfoFactory.Create(__UserId,
                    startIndex + i,
                    stub,
                    null,
                    __OptionalStateChangedHandler);
            }).Cast<IAudio>().ToList();
        }
    }
}
