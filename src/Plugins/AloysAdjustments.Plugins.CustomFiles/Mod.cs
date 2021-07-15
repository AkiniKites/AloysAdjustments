﻿using System;
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
        Normal = 1,
        OverridesFiles = 2,
        OverridenFiles = 4,
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
        public int Order { get; set; }
        public SourceType SourceType { get; set; }
        public ModFileStatus FileStatus { get; set; }
        public DateTime Added { get; set; }

        [JsonIgnore]
        public ModStatus Status { get; set; }
        [JsonIgnore]
        public ModStatus SelectedStatus { get; set; }
        [JsonIgnore]
        public long Size { get; set; }

        [JsonIgnore]
        public Dictionary<ulong, ModFile> Files { get; set; }

        public Mod()
        {
            FileStatus = ModFileStatus.Normal;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
