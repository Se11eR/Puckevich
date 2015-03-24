using System;
using System.Threading.Tasks;

namespace PuckevichCore.Interfaces
{
    public enum AudioStorageStatus
    {
        Stored,
        PartiallyStored,
        NotStored
    }

    internal interface IAudioStorage : IDisposable
    {
        ICacheStream GetCacheStream(IAudio audio);

        IAudio GetAt(long userId, int index);

        bool CheckCached(IAudio audio);

        void RemovecachedAudio(long auidiId);

        int GetCount(long userId);
    }
}
