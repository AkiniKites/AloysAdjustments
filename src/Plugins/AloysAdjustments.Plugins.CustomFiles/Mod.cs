using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Plugins.CustomFiles.Sources;
using Newtonsoft.Json;

namespace AloysAdjustments.Plugins.CustomFiles
{
    [Flags]
    public enum ModStatus
    {
        Normal,
        Suspect
    }

    public class Mod
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public SourceType SourceType { get; set; }

        [JsonIgnore]
        public ModStatus Status { get; set; }

        [JsonIgnore]
        public Dictionary<ulong, ModFile> Files { get; set; }
    }
}
