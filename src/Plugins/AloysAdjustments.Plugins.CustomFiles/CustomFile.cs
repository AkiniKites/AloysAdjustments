using System;
using System.Collections.Generic;
using System.Text;

namespace AloysAdjustments.Plugins.CustomFiles
{
    public enum SourceType
    {
        File,
        Zip,
        Pack
    }

    public class CustomFile : IComparable<CustomFile>
    {
        public string Name { get; }
        public string SourcePath { get; set; }
        public string Path { get; set; }
        public SourceType SourceType { get; set; }
        public bool Valid { get; set; }

        public CustomFile(string name, string source)
        {
            Name = name;
            SourcePath = source;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is CustomFile file &&
                   Path == file.Path;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Path);
        }

        public int CompareTo(CustomFile other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Name, other.Name);
        }
    }
}
