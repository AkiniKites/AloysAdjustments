using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Plugins.CustomFiles.Sources;

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
        public ModStatus Status { get; set; }

        public Dictionary<ulong, ModFile> Files { get; set; }
    }
}
