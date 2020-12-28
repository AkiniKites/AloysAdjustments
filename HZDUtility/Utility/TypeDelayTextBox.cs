using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HZDUtility.Utility
{
    public partial class TypeDelayTextBox : TextBox
    {
        public bool EnableTypingEvent { get; set; }

        public TypeDelayTextBox()
        {
            InitializeComponent();

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

        private void _timer_Tick(object sender, EventArgs e)
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
