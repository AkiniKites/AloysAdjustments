using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;

namespace AloysAdjustments.Plugins.Upgrades.Data
{
    public class Upgrade
    {
        public BaseGGUUID Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }

        public bool Modified { get; set; }

        public BaseGGUUID LocalNameId { get; set; }
        public string LocalNameFile { get; set; }

        private string _displayName;
        public string DisplayName => _displayName == null ? null : _displayName + (Modified ? " *" : "");

        public void SetDisplayName(string name)
        {
            _displayName = name;
        }

        public Upgrade Clone()
        {
            return new Upgrade()
            {
                Id = BaseGGUUID.FromOther(Id),
                Name = Name,
                Value = Value,
                LocalNameId = BaseGGUUID.FromOther(LocalNameId),
                LocalNameFile = LocalNameFile
            };
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
            return System.HashCode.Combine(Id);
        }
    }
}
