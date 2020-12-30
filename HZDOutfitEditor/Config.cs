using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HZDOutfitEditor
{
    public class Config
    {
        public DecimaConfig Decima { get; set; }

        public string PlayerComponentsFile { get; set; }
        public string[] OutfitFiles { get; set; }
        public string OutfitMapPath { get; set; }
        public string TempPath { get; set; }
        public string PatchFile { get; set; }
        public string PackDir { get; set; }
        
        public Settings Settings { get; set; }
    }

    public class DecimaConfig
    {
        public string Path { get; set; }
        public string Lib { get; set; }
        public string RepoUser { get; set; }
        public string RepoName { get; set; }
        public string RepoFile { get; set; }
    }

    public class Settings
    {
        public string GamePath { get; set; }
        public string LastOpen { get; set; }
    }
}
