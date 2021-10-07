using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.Common
{
    public class CommonConfig
    {
        public const string ConfigName = "Common";

        public string HumanoidMatcher { get; set; }
        public string UniqueHumanoidMatcher { get; set; }
        public string[] IgnoredFiles { get; set; }
    }
}
