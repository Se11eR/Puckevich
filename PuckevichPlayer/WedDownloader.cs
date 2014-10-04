using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using PuckevichCore;

namespace PuckevichPlayer
{
    public class WedDownloader : IWebDownloader
    {
        public Stream GetUrlStream(Uri url, out long streamLength)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            var response = (HttpWebResponse) request.GetResponse();
            var resStream = response.GetResponseStream();
            streamLength = response.ContentLength;
            return resStream;
        }

        public async Task<Tuple<Stream, long>> GetUrlStreamAsync(Uri url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse) await request.GetResponseAsync();
            var resStream = response.GetResponseStream();

            return new Tuple<Stream, long>(resStream, response.ContentLength);
        }
    }
}