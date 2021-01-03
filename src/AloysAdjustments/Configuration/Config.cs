using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AloysAdjustments.Configuration
{
    public class Config
    { 
        public Dictionary<string, JObject> ModuleConfigs { get; set; }

        public string UpgradeFile { get; set; }
        public string[] IgnoredUpgrades { get; set; }
        public string PrefetchFile { get; set; }

        public string ArchiverLib { get; set; }
        public string TempPath { get; set; }
        public string PatchFile { get; set; }
        public string PackDir { get; set; }
    }
}
