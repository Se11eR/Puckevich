using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuckevichCore.DataVirtualization;

namespace PuckevichPlayer.Virtualizing
{
    internal class ItemsProviderCollectionWrapper<T> : ICollection<T>
    {
        private readonly IItemsProvider<T> __Ip;

        public ItemsProviderCollectionWrapper(IItemsProvider<T> ip)
        {
            __Ip = ip;
        }

        public IEnumerator<T> GetEnumerator()
        {
            const int fetchCount = 10;
            for (int i = 0; i < Count; i += fetchCount)
            {
                var fetched = __Ip.FetchRange(i, Math.Min(fetchCount, Count - i));
                foreach (var x in fetched)
                {
                    yield return x;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return __Ip.FetchCount(); }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }
    }
}
