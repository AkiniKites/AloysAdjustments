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
using AloysAdjustments.Utility;

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

        public void Download(T source, string filePath, Action<bool, bool, byte[]> callback)
        {
            var path = Path.GetFullPath(filePath);
            var cacheLock = _cacheLocks.GetOrAdd(path, x => new ReaderWriterLockSlim());

            //TODO: Refactor for async
            void downloadFile()
            {
                try
                {
                    var bytes = GetExisting(path, cacheLock);
                    if (bytes != null)
                    {
                        callback(true, false, bytes);
                        return;
                    }
                    
                    bytes = DownloadFile(source);
                    using (cacheLock.UsingWriterLock())
                    {
                        Paths.CheckDirectory(Path.GetDirectoryName(filePath));
                        File.WriteAllBytes(filePath, bytes);
                    }
                    callback(true, true, bytes);
                }
                catch (Exception)
                {
                    callback(false, false, null);
                }
            }

            GetExistingAsync(path, cacheLock, (success, bytes) =>
            {
                if (success)
                {
                    callback(true, false, bytes);
                    return;
                }

                _requests.Push(downloadFile);
            });
        }

        private void GetExistingAsync(string filePath, ReaderWriterLockSlim cacheLock, Action<bool, byte[]> callback)
        {
            Task.Run(() =>
            {
                using (cacheLock.UsingReaderLock())
                {
                    if (File.Exists(filePath))
                    {
                        var bytes = File.ReadAllBytes(filePath);
                        callback(true, bytes);
                        return;
                    }
                }

                callback(false, null);
            });
        }
        private byte[] GetExisting(string filePath, ReaderWriterLockSlim cacheLock)
        {
            using (cacheLock.UsingReaderLock())
            {
                if (File.Exists(filePath))
                    return File.ReadAllBytes(filePath);
            }

            return null;
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
