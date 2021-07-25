using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Common.Utility;
using Decima.HZD;

namespace AloysAdjustments.Common
{
    public class Downloader
    {
        private readonly LimitedConcurrentStack<Action> _requests;

        public Downloader()
        {
            _requests = new LimitedConcurrentStack<Action>(10);

            Task.Run(DownloadWorker);
        }

        public void Shutdown()
        {
            _requests.CompleteAdding();
        }

        public void Download(string url, string filePath, Action<bool> callback)
        {
            _requests.Push(() =>
            {
                //download
                Console.WriteLine(url);
                callback(true);
            });
        }

        private void DownloadWorker()
        {
            while (_requests.TryPopWait(out var request))
            {
                request();
            }
        }
    }
}
