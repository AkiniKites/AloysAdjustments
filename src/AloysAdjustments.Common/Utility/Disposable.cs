using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Common.Utility
{
    public class Disposable
    {
        private class DisposableWrapper<T> : IDisposable
        {
            private readonly T _obj;
            private readonly Action<T> _exit;

            public DisposableWrapper(T obj, Action<T> exit)
            {
                _obj = obj;
                _exit = exit;
            }

            public void Dispose()
            {
                _exit(_obj);
            }
        }
        
        public static IDisposable Create<T>(T obj, Action<T> enter, Action<T> exit)
        {
            enter(obj);

            return new DisposableWrapper<T>(obj, exit);
        }
        public static async Task<IDisposable> CreateAsync<T>(T obj, Func<T, Task> enter, Action<T> exit)
        {
            await enter(obj);

            return new DisposableWrapper<T>(obj, exit);
        }
    }
}
