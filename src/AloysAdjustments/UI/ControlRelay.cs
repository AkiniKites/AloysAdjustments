using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Common.UI;

namespace AloysAdjustments.UI
{
    public delegate void EmptyEventHandler();

    public class ControlRelay : INotifyPropertyChanged
    {
        public event EmptyEventHandler Click;
        public event PropertyChangedEventHandler PropertyChanged;

        public AsyncRelayCommand ClickCommand { get; }

        public bool Enabled { get; set; } = true;

        public ControlRelay()
            : this(() => Task.CompletedTask) { }
        public ControlRelay(Func<Task> command)
        {
            ClickCommand = new AsyncRelayCommand(command);
        }

        public void OnClick()
        {
            Click?.Invoke();
        }
    }
}
