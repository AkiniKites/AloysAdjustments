using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.NPC
{
    public class ValuePair<T> : INotifyPropertyChanged
    {
        public T Default { get; set; }
        public T Value { get; set; }

        public bool Modified => !EqualityComparer<T>.Default.Equals(Default, Value);

        public ValuePair(T defaultValue, T value)
        {
            Default = defaultValue;
            Value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}