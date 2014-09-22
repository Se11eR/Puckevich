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
        private readonly IItemsProvider<IAudio> __InternalProvide;

        public AudioModelProviderWrapper(IItemsProvider<IAudio> internalProvide)
        {
            __InternalProvide = internalProvide;
        }

        public int FetchCount()
        {
            return __InternalProvide.FetchCount();
        }

        public IList<AudioModel> FetchRange(int startIndex, int count)
        {
            return __InternalProvide.FetchRange(startIndex, count).Select(audio => new AudioModel(audio)).ToList();
        }
    }
}
