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
            _requests.ItemDropped += Requests_ItemDropped;

            Task.Run(DownloadWorker);
        }

        private void Requests_ItemDropped(Action item)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            _requests.CompleteAdding();
        }

        public void Download(T source, string filePath, Action<bool, byte[]> callback)
        {
            var path = Path.GetFullPath(filePath);
            var cacheLock = _cacheLocks.GetOrAdd(path, x => new ReaderWriterLockSlim());

            //TODO: Refactor for async
            void downloadFile()
            {
                try
                {
                    GetExisting(path, cacheLock, (success, bytes) =>
                    {
                        if (success)
                        {
                            callback(true, bytes);
                            return;
                        }

                        bytes = DownloadFile(source);
                        using (cacheLock.UsingWriterLock())
                        {
                            Paths.CheckDirectory(Path.GetDirectoryName(filePath));
                            File.WriteAllBytes(filePath, bytes);
                        }
                        callback(true, bytes);
                    });
                }
                catch (Exception)
                {
                    callback(false, null);
                }
            }

            GetExisting(path, cacheLock, (success, bytes) =>
            {
                if (success)
                {
                    callback(true, bytes);
                    return;
                }

                _requests.Push(downloadFile);
            });
        }

        private void GetExisting(string filePath, ReaderWriterLockSlim cacheLock, Action<bool, byte[]> callback)
        {
            Task.Run(() =>
            {
                using (cacheLock.UsingReaderLock())
                {
                    Thread.Sleep(1000);
                    if (File.Exists(filePath))
                    {
                        var bytes = File.ReadAllBytes(filePath);
                        callback(true, bytes);
                    }
                }

                callback(false, null);
            });
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
