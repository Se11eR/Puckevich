using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuckevichCore;

namespace PuckevichPlayer
{
    public class AudioModelProviderWrapper : IItemsProvider<AudioModel>
    {
        private readonly IItemsProvider<IAudio> __InternalProvider;

        public AudioModelProviderWrapper(IItemsProvider<IAudio> internalProvider)
        {
            __InternalProvider = internalProvider;
        }

        public int FetchCount()
        {
            return __InternalProvider.FetchCount();
        }

        public IList<AudioModel> FetchRange(int startIndex, int count)
        {
            return __InternalProvider.FetchRange(startIndex, count).Select(audio => new AudioModel(audio)).ToList();
        }
    }
}
