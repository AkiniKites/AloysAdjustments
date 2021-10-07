using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AloysAdjustments.Common.Downloads
{
    public class HttpDownloader : ThrottledDownloader<string>
    {
        private readonly WebClient _client;

        public HttpDownloader(int maxStackSize)
            : base(maxStackSize)
        {
            _client = new WebClient();
        }

        protected override byte[] DownloadFile(string url)
        {
            return _client.DownloadData(url);
        }
    }
}
