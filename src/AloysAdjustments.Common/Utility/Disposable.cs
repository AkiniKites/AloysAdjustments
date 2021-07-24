using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Common.Utility
{
    public class Disposable<T> : IDisposable
    {
        private readonly T _obj;
        private readonly Action<T> _exit;

        public Disposable(T obj, Action<T> enter, Action<T> exit)
        {
            _obj = obj;
            _exit = exit;

            enter(obj);
        }

        public void Dispose()
        {
            _exit(_obj);
        }
    }
}
