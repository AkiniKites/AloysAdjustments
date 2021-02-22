using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Configuration
{
    public class UserSettings : INotifyPropertyChanged
    {
        public UserSettings()
        {
            Windows = new Dictionary<string, Rectangle>();
        }

        public string GamePath { get; set; }
        public string LastPackOpen { get; set; }
        public bool ApplyToAllOutfits { get; set; }
        public int OutfitModelFilter { get; set; }
        public bool ApplyToAllNpcs { get; set; }
        public int NpcModelFilter { get; set; }

        public Dictionary<string, Rectangle> Windows { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
