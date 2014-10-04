using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuckevichCore;

namespace PuckevichPlayer.Virtualizing
{
    internal class PuckevichVirtualizingCollection : AsyncVirtualizingCollection<AudioModel>
    {
        public PuckevichVirtualizingCollection(IItemsProvider<AudioModel> itemsProvider)
            : base(itemsProvider)
        {
        }

        public PuckevichVirtualizingCollection(IItemsProvider<AudioModel> itemsProvider, int pageSize)
            : base(itemsProvider, pageSize)
        {
        }

        public PuckevichVirtualizingCollection(IItemsProvider<AudioModel> itemsProvider, int pageSize, int pageTimeout)
            : base(itemsProvider, pageSize, pageTimeout)
        {
        }

        public async Task StopAllAsync()
        {
            foreach (
                var model in
                    _pages.SelectMany(
                                      pair =>
                                      pair.Value.Where(
                                                       model =>
                                                       model.AudioState == PlayingState.Playing || model.AudioState == PlayingState.Paused))
                )
            {
                await model.Stop();
            }
        }
    }
}
