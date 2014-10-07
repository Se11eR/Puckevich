namespace PuckevichPlayer.Storage
{
    internal class JsonAudioModel
    {
        public long AudioId { get; set; }

        public long UserId { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public int Duration { get; set; }

        public long FileSize { get; set; }
    }
}