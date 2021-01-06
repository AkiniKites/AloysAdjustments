using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;

namespace AloysAdjustments.Data
{
    public class Model
    {
        public BaseGGUUID Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public string DisplayName { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is Model model &&
                EqualityComparer<BaseGGUUID>.Default.Equals(Id, model.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}