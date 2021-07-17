using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;

namespace AloysAdjustments.Plugins.Upgrades.Data
{
    public class AmmoItem
    {
        public BaseGGUUID Id { get; set; }
        public string File { get; set; }

        public string FactFile { get; set; }
        public BaseGGUUID FactId { get; set; }

        public override string ToString()
        {
            return $"{File}: {Id}";
        }
    }
}
