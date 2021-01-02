using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Configuration
{
    public class Config
    { 
        public DecimaConfig Decima { get; set; }

        public string PlayerComponentsFile { get; set; }
        public string[] OutfitFiles { get; set; }
        public string UpgradeFile { get; set; }
        public string[] IgnoredUpgrades { get; set; }


        public string PackagerLib { get; set; }
        public string TempPath { get; set; }
        public string PatchFile { get; set; }
        public string PackDir { get; set; }
    }

    public class DecimaConfig
    {
        public string Path { get; set; }
        public string Lib { get; set; }
        public string RepoUser { get; set; }
        public string RepoName { get; set; }
        public string RepoFile { get; set; }
    }
}
