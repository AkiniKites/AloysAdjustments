using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;

namespace HZDOutfitEditor.Models
{
    public class Model
    {
        public BaseGGUUID Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            var key = "playercostume_";
            var idx = Name.LastIndexOf(key);

            if (idx >= 0)
                return Name.Substring(idx + key.Length);

            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is Model model &&
                   EqualityComparer<BaseGGUUID>.Default.Equals(Id, model.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}