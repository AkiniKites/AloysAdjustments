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

        public string PrefetchFile { get; set; }

        public string ArchiverLib { get; set; }
        public string TempPath { get; set; }
        public string CachePath { get; set; }
        public string ImagesPath { get; set; }
        public string PatchFile { get; set; }
        public string PackDir { get; set; }
        public string[] OldVersionsToDelete { get; set; }
        public string UpdatesRepo { get; set; }
        public string ImagesDb { get; set; }
        public string[] KnownGameFiles { get; set; }
        public List<string> PluginLoadOrder { get; set; }
    }
}
