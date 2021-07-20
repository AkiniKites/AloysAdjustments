using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Utility;
using Decima;

namespace AloysAdjustments.Plugins.AmmoUpgrades.Data
{
    public class AmmoItem
    {
        public bool Upgradeable { get; set; }

        public string Name { get; set; }
        public LocalString LocalName { get; set; }

        public BaseGGUUID MainId { get; set; }
        public string MainFile { get; set; }

        public BaseGGUUID StackableId { get; set; }
        public string StackableFile { get; set; }

        public BaseGGUUID FactId { get; set; }
        public string FactFile { get; set; }

        public override string ToString()
        {
            return $"{Name} - {StackableFile}";
        }
    }
}
