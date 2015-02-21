using System;
using System.IO;
using System.Threading.Tasks;

namespace PuckevichCore.Interfaces
{
    public interface IWebDownloader
    {
        Stream GetUrlStream(Uri url, long startFrom, out long streamLength);

        Task<Tuple<Stream, long>> GetUrlStreamAsync(Uri url, long startFrom);
    }
}
