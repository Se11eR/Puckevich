using System.Threading.Tasks;

namespace PuckevichCore.Interfaces
{
    public delegate void PlayingStateChangedEventHandler(IManagedPlayable sender);
    public delegate void PercentsDownloadedChangedEventHandler(IManagedPlayable sender);
    public delegate void SecondsPlayedChangedEventHandler(IManagedPlayable sender);

    public interface IManagedPlayable
    {

        Task PlayAsync();

        Task PauseAsync();

        void Stop();

        Task StopAsync();

        int SecondsPlayed { get; }

        double PercentsDownloaded { get; }

        event PlayingStateChangedEventHandler PlayingStateChanged;
        event PercentsDownloadedChangedEventHandler PercentsDownloadedChanged;
        event SecondsPlayedChangedEventHandler SecondsPlayedChanged;

        PlayingState State { get; }
    }
}
