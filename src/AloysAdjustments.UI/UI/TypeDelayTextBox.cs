using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Windows.Controls;

namespace AloysAdjustments.WPF.UI
{
    public class TypeDelayTextBox : TextBox
    {
        private readonly Timer _timer = new Timer();

        public bool EnableTypingEvent { get; set; }

        public TypeDelayTextBox()
        {
            _timer.Interval = 500;
            _timer.Elapsed += _timer_Elapsed;

            EnableTypingEvent = true;
            this.TextChanged += OnTextChanged;
        }

        private void OnTextChanged(object sender, EventArgs args)
        {
            if (EnableTypingEvent)
            {
                _timer.Enabled = true;
                _timer.Stop();
                _timer.Start();
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            OnTypingFinished();
        }

        public event EventHandler TypingFinished;

        protected virtual void OnTypingFinished()
        {
            TypingFinished?.Invoke(this, EventArgs.Empty);
        }
    }
}
