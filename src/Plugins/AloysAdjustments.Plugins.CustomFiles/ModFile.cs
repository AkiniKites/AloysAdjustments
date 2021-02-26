using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AloysAdjustments.Plugins.CustomFiles
{
    public enum FileStatus
    {
        Invalid,
        Known,
        Normal,
        Suspect
    }

    public class ModFile : IComparable<ModFile>, INotifyPropertyChanged
    {
        public string Name { get; set; }
        public ulong Hash { get; set; }
        public string Path { get; set; }

        public FileStatus Status { get; set; }

        public ModFile() { }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(ModFile other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Name, other.Name);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
