using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        OverridesFiles,
        OverridenFiles,
    }
    
    public enum ModFileStatus
    {
        Normal,
        Suspect,
    }

    public class Mod : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public SourceType SourceType { get; set; }
        public ModFileStatus FileStatus { get; set; }

        [JsonIgnore]
        public ModStatus Status { get; set; }

        [JsonIgnore]
        public Dictionary<ulong, ModFile> Files { get; set; }

        public Mod()
        {
            FileStatus = ModFileStatus.Normal;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
