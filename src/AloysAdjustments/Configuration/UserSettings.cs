using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Configuration
{
    public class UserSettings : INotifyPropertyChanged
    {
        public string GamePath { get; set; }
        public string LastPackOpen { get; set; }

#pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067
    }
}
