using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AloysAdjustments.Common.Utility
{
    //Taken from https://stackoverflow.com/questions/19659387/readerwriterlockslim-and-async-await
    public class AsyncReaderWriterLock : IDisposable
    {
        private readonly SemaphoreSlim _readSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _writeSemaphore = new SemaphoreSlim(1, 1);
        private int _readerCount;

        public async Task EnterWriteLock(CancellationToken token = default)
        {
            await _writeSemaphore.WaitAsync(token).ConfigureAwait(false);
            await SafeAcquireReadSemaphore(token).ConfigureAwait(false);
        }

        public void ExitWriteLock()
        {
            _readSemaphore.Release();
            _writeSemaphore.Release();
        }

        public async Task EnterReadLock(CancellationToken token = default)
        {
            await _writeSemaphore.WaitAsync(token).ConfigureAwait(false);

            if (Interlocked.Increment(ref _readerCount) == 1)
            {
                try
                {
                    await SafeAcquireReadSemaphore(token).ConfigureAwait(false);
                }
                catch
                {
                    Interlocked.Decrement(ref _readerCount);
                    throw;
                }
            }

            _writeSemaphore.Release();
        }

        public void ExitReadLock()
        {
            if (Interlocked.Decrement(ref _readerCount) == 0)
                _readSemaphore.Release();
        }

        private async Task SafeAcquireReadSemaphore(CancellationToken token)
        {
            try
            {
                await _readSemaphore.WaitAsync(token).ConfigureAwait(false);
            }
            catch
            {
                _writeSemaphore.Release();
                throw;
            }
        }

        public void Dispose()
        {
            _writeSemaphore.Dispose();
            _readSemaphore.Dispose();
        }
    }
}
