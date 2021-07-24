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
            new Disposable<ReaderWriterLockSlim>(lockSlim, x => x.EnterReadLock(), x => x.ExitReadLock());
        public static IDisposable UsingWriterLock(this ReaderWriterLockSlim lockSlim) =>
            new Disposable<ReaderWriterLockSlim>(lockSlim, x => x.EnterWriteLock(), x => x.ExitWriteLock());
    }
}
