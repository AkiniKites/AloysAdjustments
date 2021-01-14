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

        public string LocalName { get; set; }
        
        public string DisplayName => LocalName == null ? null : LocalName + (Modified ? " *" : "");

        public Outfit Clone()
        {
            return new Outfit()
            {
                ModelId = BaseGGUUID.FromOther(ModelId),
                ModelFile = ModelFile,
                Name = Name,
                RefId = BaseGGUUID.FromOther(RefId),
                SourceFile = SourceFile,
                LocalName = LocalName
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
