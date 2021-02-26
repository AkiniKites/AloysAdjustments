using System;
using System.Collections.Generic;
using AloysAdjustments.Logic.Patching;

namespace AloysAdjustments.Plugins.CustomFiles.Sources
{
    public enum SourceType
    {
        Dir,
        Zip,
        Pack
    }

    public abstract class Source
    {
        public Func<ulong, string, FileStatus> GetFileStatus { get; set; }
        public Func<ulong, string> GetFileName { get; set; }

        public abstract SourceType SourceType { get; }
        public abstract Mod TryLoad(string path);
    }
}
