using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AloysAdjustments.Common.Utility
{
    public class DelayAction
    {
        private readonly object _lock = new object();

        private readonly Action _action;
        private bool _running;
        
        public DelayAction(Action action)
        {
            _action = action;
        }

        public void Start(int ms) => Start(TimeSpan.FromMilliseconds(ms));
        public void Start(TimeSpan time)
        {
            lock (_lock)
            {
                if (!_running)
                    _running = true;
                else
                    return;
            }

            Task.Run(() =>
            {
                Thread.Sleep(time);

                lock (_lock)
                {
                    if (_running)
                        _action();
                    _running = false;
                }
            });
        }

        public void Complete()
        {
            lock (_lock)
            {
                _running = false;
            }
        }
    }
}
