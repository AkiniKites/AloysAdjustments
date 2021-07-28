using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AloysAdjustments.Common.Utility;

namespace AloysAdjustments.Common.Downloads
{
    public abstract class ThrottledDownloader<T> : IDisposable
    {
        private readonly LimitedConcurrentStack<Action> _requests;

        private static readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _cacheLocks
            = new ConcurrentDictionary<string, ReaderWriterLockSlim>(StringComparer.OrdinalIgnoreCase);

        protected ThrottledDownloader(int maxStackSize)
        {
            _requests = new LimitedConcurrentStack<Action>(maxStackSize);

            Task.Run(DownloadWorker);
        }

        public void Shutdown()
        {
            _requests.CompleteAdding();
        }

        public void Download(T source, string filePath, Action<bool, byte[]> callback)
        {
            var path = Path.GetFullPath(filePath);
            var cacheLock = _cacheLocks.GetOrAdd(path, x => new ReaderWriterLockSlim());
            if (GetExisting(path, cacheLock, callback))
                return;

            _requests.Push(() =>
            {
                try
                {
                    if (GetExisting(path, cacheLock, callback))
                        return;
                    
                    var bytes = DownloadFile(source);
                    using (cacheLock.UsingWriterLock())
                        File.WriteAllBytes(filePath, bytes);
                    callback(true, bytes);
                }
                catch (Exception)
                {
                    callback(false, null);
                }
            });
        }

        private bool GetExisting(string filePath, ReaderWriterLockSlim cacheLock, Action<bool, byte[]> callback)
        {
            using (cacheLock.UsingReaderLock())
            {
                if (File.Exists(filePath))
                {
                    var bytes = File.ReadAllBytes(filePath);
                    callback(true, bytes);
                    return true;
                }
            }

            return false;
        }

        private void DownloadWorker()
        {
            while (_requests.TryPopWait(out var request))
            {
                request();
            }
        }

        protected abstract byte[] DownloadFile(T source);

        public void Dispose()
        {
            Shutdown();
        }
    }
}
