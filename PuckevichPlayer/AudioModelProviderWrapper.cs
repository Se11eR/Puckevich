using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuckevichCore;
using PuckevichCore.DataVirtualization;
using PuckevichCore.Interfaces;

namespace PuckevichPlayer
{
    public class AudioModelProviderWrapper : IItemsProvider<AudioModel>
    {
        private readonly IItemsProvider<IAudio> __InternalProvider;
        private readonly Func<AudioModel> __GetCurrentActive;

        public AudioModelProviderWrapper(IItemsProvider<IAudio> internalProvider, Func<AudioModel> getCurrentActive)
        {
            __InternalProvider = internalProvider;
            __GetCurrentActive = getCurrentActive;
        }

        public int FetchCount()
        {
            return __InternalProvider.FetchCount();
        }

        public IList<AudioModel> FetchRange(int startIndex, int count)
        {
            return __InternalProvider.FetchRange(startIndex, count).Select(audio =>
            {
                var cur = __GetCurrentActive();
                return cur != null && audio.AudioId == cur.AudioId ? cur : new AudioModel(audio);
            }).ToList();
        }
    }
}
