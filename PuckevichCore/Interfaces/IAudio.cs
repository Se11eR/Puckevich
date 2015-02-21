namespace PuckevichCore.Interfaces
{
    public interface IAudio
    {
        long AudioId { get; }

        long UserId { get; }

        string Title { get; }

        string Artist { get; }

        int Duration { get; }

        IManagedPlayable Playable { get; }
    }
}