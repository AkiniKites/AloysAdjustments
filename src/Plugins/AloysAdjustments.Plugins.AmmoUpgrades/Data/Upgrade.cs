using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Utility;
using Decima;

namespace AloysAdjustments.Plugins.AmmoUpgrades.Data
{
    public class Upgrade : INotifyPropertyChanged
    {
        public BaseGGUUID Id { get; set; }
        public string File { get; set; }

        public string Name { get; set; }
        public LocalString LocalName { get; set; }

        public string Category { get; set; }
        public LocalString LocalCategory { get; set; }
        
        public int Level { get; set; }
        
        public int DefaultValue { get; set; }
        public int Value { get; set; }

        public bool Modified => DefaultValue != Value;

        private string _displayName;
        public string DisplayName => _displayName == null ? null : _displayName + (Modified ? " *" : "");
        public string DisplayCategory { get; set; }

        public void SetDisplayName(string name)
        {
            _displayName = name;
        }

        public override string ToString()
        {
            return DisplayName ?? Name;
        }

        public override bool Equals(object obj)
        {
            return obj is Upgrade upgrade &&
                EqualityComparer<BaseGGUUID>.Default.Equals(Id, upgrade.Id);
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
