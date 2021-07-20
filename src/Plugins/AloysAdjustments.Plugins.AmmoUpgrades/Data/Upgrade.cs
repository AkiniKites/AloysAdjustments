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
    public class AmmoUpgrade : INotifyPropertyChanged
    {
        public BaseGGUUID Id { get; set; }
        public string File { get; set; }

        public string Name { get; set; }
        public LocalString LocalName { get; set; }

        public string Category { get; set; }
        public LocalString LocalCategory { get; set; }

        public int Sort { get; set; }
        
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
            return obj is AmmoUpgrade upgrade &&
                   EqualityComparer<BaseGGUUID>.Default.Equals(Id, upgrade.Id) &&
                   Level == upgrade.Level;
        }

        public override int GetHashCode()
        {
            int hashCode = -1089996163;
            hashCode = hashCode * -1521134295 + EqualityComparer<BaseGGUUID>.Default.GetHashCode(Id);
            hashCode = hashCode * -1521134295 + Level.GetHashCode();
            return hashCode;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
