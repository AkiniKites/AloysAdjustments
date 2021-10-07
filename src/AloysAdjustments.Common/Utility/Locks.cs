using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AloysAdjustments.Common.Utility
{
    public static class Locks
    {
        public static IDisposable UsingReaderLock(this ReaderWriterLockSlim lockSlim) =>
            Disposable.Create(lockSlim, x => x.EnterReadLock(), x => x.ExitReadLock());
        public static IDisposable UsingWriterLock(this ReaderWriterLockSlim lockSlim) =>
            Disposable.Create(lockSlim, x => x.EnterWriteLock(), x => x.ExitWriteLock());

        public static async Task<IDisposable> UsingReaderLock(this AsyncReaderWriterLock asyncLock) =>
            await Disposable.CreateAsync(asyncLock, x => x.EnterReadLock(), x => x.ExitReadLock());
        public static async Task<IDisposable> UsingWriterLock(this AsyncReaderWriterLock asyncLock) =>
            await Disposable.CreateAsync(asyncLock, x => x.EnterWriteLock(), x => x.ExitWriteLock());
    }
}
