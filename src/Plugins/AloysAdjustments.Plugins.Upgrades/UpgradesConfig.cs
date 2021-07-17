using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.Upgrades
{
    public class UpgradeConfig
    {
        public string AmmoMatcher { get; set; }
        public string UpgradeFile { get; set; }
        public string[] IgnoredUpgrades { get; set; }
    }
}
