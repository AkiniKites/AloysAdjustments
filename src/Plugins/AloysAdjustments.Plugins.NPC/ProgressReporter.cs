using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AloysAdjustments.Logic;

namespace AloysAdjustments.Plugins.NPC
{
    public class ProgressReporter : IDisposable
    {
        private readonly int _maxValue;
        private readonly bool _hideOnComplete;
        private int _current;
        private int _lastNotify;
        private bool _disposed;

        public ProgressReporter (int maxValue, bool hideOnComplete = true)
        {
            _maxValue = maxValue;
            _hideOnComplete = hideOnComplete;
        }

        public void Tick()
        {
            if (_disposed)
                throw new Exception("ProgressReporter is disposed");

            Interlocked.Increment(ref _current);

            var notify = _current * 100 / _maxValue;
            if (notify> _lastNotify)
            {
                _lastNotify = notify;
                IoC.Notif.ShowProgress(notify / 100.0 / _maxValue);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                throw new Exception("ProgressReporter is already disposed");
            _disposed = true;

            if (_hideOnComplete)
                IoC.Notif.HideProgress();
            else
                IoC.Notif.ShowUnknownProgress();
        }
    }
}