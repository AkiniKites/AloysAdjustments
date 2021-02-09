using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace AloysAdjustments.UI
{
    public delegate void EmptyEventHandler();

    public class ControlRelay : INotifyPropertyChanged
    {
        public event EmptyEventHandler Click;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool Enabled { get; set; } = true;
        
        public void OnClick()
        {
            Click?.Invoke();
        }
    }
}
