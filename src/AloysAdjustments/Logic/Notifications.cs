using System;
using System.Collections.Generic;
using System.Text;

namespace AloysAdjustments.Logic
{
    public class Notifications
    {
        private readonly Action<string, bool> _statusSetter;
        private readonly Action<int, int, bool, bool> _progressSetter;

        public Action CacheUpdate { get; set; }

        public Notifications(Action<string, bool> statusSetter, 
            Action<int, int, bool, bool> progressSetter)
        {
            _statusSetter = statusSetter;
            _progressSetter = progressSetter;
        }

        public void ShowStatus(string text)
        {
            _statusSetter(text, false);
        }
        public void ShowError(string text)
        {
            _statusSetter(text, true);
        }
        public void ShowProgress(double value)
        {
            _progressSetter((int)Math.Round(value * 100), 100, false, true);
        }
        public void ShowUnknownProgress()
        {
            _progressSetter(0, 0, true, true);
        }
        public void HideProgress()
        {
            _progressSetter(0, 0, false, false);
        }
    }
}
