using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;

namespace AloysAdjustments.Data
{
    public class Outfit
    {
        public BaseGGUUID ModelId { get; set; }
        public string ModelFile { get; set; }

        public string Name { get; set; }
        public bool Modified { get; set; }

        public BaseGGUUID RefId { get; set; }
        public string SourceFile {get; set;}

        public BaseGGUUID LocalNameId { get; set; }
        public string LocalNameFile { get; set; }

        private string _displayName;
        public string DisplayName => _displayName == null ? null : _displayName + (Modified ? " *" : "");

        public void SetDisplayName(string name)
        {
            _displayName = name;
        }

        public Outfit Clone()
        {
            return new Outfit()
            {
                ModelId = BaseGGUUID.FromOther(ModelId),
                ModelFile = ModelFile,
                Name = Name,
                RefId = BaseGGUUID.FromOther(RefId),
                SourceFile = SourceFile,
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
            return obj is Outfit outfit &&
                   EqualityComparer<BaseGGUUID>.Default.Equals(RefId, outfit.RefId);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(RefId);
        }
    }
}
