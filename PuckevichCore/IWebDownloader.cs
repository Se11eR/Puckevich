using System;
using System.IO;
using System.Threading.Tasks;

namespace PuckevichCore
{
    public interface IWebDownloader
    {
        Stream GetUrlStream(Uri url, out long streamLength);

        Task<Tuple<Stream, long>> GetUrlStreamAsync(Uri url);
    }
}
