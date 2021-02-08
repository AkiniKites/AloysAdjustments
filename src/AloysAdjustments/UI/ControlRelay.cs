using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;

namespace AloysAdjustments.UI
{
    public delegate void EmptyEventHandler();

    public class ControlRelay : INotifyPropertyChanged
    {
        public event EmptyEventHandler Click;
        public event PropertyChangedEventHandler PropertyChanged;

        public AsyncRelayCommand ClickCommand { get; }

        public bool Enabled { get; set; } = true;

        public ControlRelay() { }
        public ControlRelay(Func<Task> onClick)
        {
            ClickCommand = new AsyncRelayCommand(onClick);
        }

        public void OnClick()
        {
            Click?.Invoke();
        }
    }
}
