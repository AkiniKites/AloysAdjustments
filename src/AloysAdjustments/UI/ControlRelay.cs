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
        
        public Action<string, object> PropertyValueChanged { get; set; }
        
        public bool Enabled { get; set; }

        public void FirePropertyChanges()
        {
            foreach (var pi in typeof(ControlRelay).GetProperties())
                OnPropertyChanged(pi.Name, null, pi.GetValue(this));
        }

        public void OnClick()
        {
            Click?.Invoke();
        }

        public void OnPropertyChanged(string propertyName, object before, object after)
        {
            PropertyValueChanged?.Invoke(propertyName, after);
        }
    }
}
