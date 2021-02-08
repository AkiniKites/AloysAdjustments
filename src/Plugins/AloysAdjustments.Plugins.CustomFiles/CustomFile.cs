using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AloysAdjustments.Plugins.CustomFiles
{
    public enum SourceType
    {
        File,
        Zip,
        Pack
    }

    public class CustomFile : IComparable<CustomFile>, INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string SubPath { get; set; }
        public SourceType SourceType { get; set; }
        public bool Valid { get; set; }

        public CustomFile() { }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(CustomFile other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Name, other.Name);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
