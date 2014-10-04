using System;
using System.IO;
using System.Net;
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
    }
}