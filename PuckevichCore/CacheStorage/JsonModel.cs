using PuckevichCore.Interfaces;

namespace PuckevichCore.CacheStorage
{
    internal class JsonAudioModel : IAudio
    {
        public long AudioId { get; set; }

        public long UserId { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public int Duration { get; set; }

        public long AudioSize { get; set; }

        public int Index { get; set; }

        public IManagedPlayable Playable
        {
            get { return null; }
        }
    }
}