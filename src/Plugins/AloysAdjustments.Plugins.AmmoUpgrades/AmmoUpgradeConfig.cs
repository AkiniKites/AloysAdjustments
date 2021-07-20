using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.AmmoUpgrades
{
    public class AmmoUpgradeConfig
    {
        public string UpgradeFile { get; set; }
        public string AmmoMatcher { get; set; }
        public string[] CombinedFiles { get; set; }
        public string[] IgnoredFiles { get; set; }
    }
}
