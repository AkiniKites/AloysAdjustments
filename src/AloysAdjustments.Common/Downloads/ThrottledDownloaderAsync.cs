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
        private readonly LimitedConcurrentStack<Action<bool>> _requests;

        private static readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _cacheLocks
            = new ConcurrentDictionary<string, ReaderWriterLockSlim>(StringComparer.OrdinalIgnoreCase);

        protected ThrottledDownloader(int maxStackSize)
        {
            _requests = new LimitedConcurrentStack<Action<bool>>(maxStackSize);
            _requests.ItemDropped += Requests_ItemDropped;

            Task.Run(DownloadWorker);
        }

        private void Requests_ItemDropped(Action<bool> action)
        {
            action(true);
        }

        public void Shutdown()
        {
            _requests.CompleteAdding();
        }
        
        public byte[] Download(T source, string filePath)
        {
            var path = Path.GetFullPath(filePath);
            var cacheLock = _cacheLocks.GetOrAdd(path, x => new ReaderWriterLockSlim());

            var bytes = GetExisting(filePath, cacheLock);
            if (bytes != null) 
                return bytes;

            var slimLock = new SemaphoreSlim(0);

            _requests.Push(dropped =>
            {
                if (!dropped)
                {
                    try
                    {
                        bytes = GetExisting(filePath, cacheLock);
                        if (bytes != null)
                        {
                            slimLock.Release();
                            return;
                        }

                        bytes = DownloadFile(source);
                        using (cacheLock.UsingWriterLock())
                        {
                            Paths.CheckDirectory(Path.GetDirectoryName(filePath));
                            File.WriteAllBytes(filePath, bytes);
                        }
                    }
                    catch (Exception) { }
                }

                slimLock.Release();
            });
            
            slimLock.Wait();
            return bytes;
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
                request(false);
            }
        }

        protected abstract byte[] DownloadFile(T source);

        public void Dispose()
        {
            Shutdown();
        }
    }
}
