using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.CustomFiles.Sources;
using AloysAdjustments.Plugins.CustomFiles.Utility;
using Decima;
using Newtonsoft.Json;

namespace AloysAdjustments.Plugins.CustomFiles
{
    public class ModFileLoader
    {
        private readonly HashSet<string> KnownRootPaths = new HashSet<string>();
        private readonly Dictionary<ulong, (string Name, bool Valid)> CoreHashes
            = new Dictionary<ulong, (string Name, bool Valid)>();
        //mostly guesses
        private readonly Dictionary<ulong, (string Name, bool Valid)> StreamHashes
            = new Dictionary<ulong, (string Name, bool Valid)>();

        private readonly Source[] Sources;

        public ModFileLoader()
        {
            Sources = new Source[] {
                new DirSource(),
                new PakSource(),
                new ZipSource(),
            };

            foreach (var source in Sources)
            {
                source.GetFileStatus = GetFileStatus;
                source.GetFileName = x => FindFileHash(x).Name;
            }
        }

        public void Initialize()
        {
            var prefetch = Prefetch.Load();

            foreach (var file in prefetch.Files.Keys)
            {
                CoreHashes.Add(Packfile.GetHashForPath(file), (file, true));
                StreamHashes.Add(Packfile.GetHashForPath(file, true), (file, true));

                var idx = file.IndexOf('/');
                if (idx > 0)
                    KnownRootPaths.Add(file.Substring(0, idx + 1));
            }

            //prefetch not valid, must be generated
            var prefetchFile = IoC.Config.PrefetchFile;
            CoreHashes.Add(Packfile.GetHashForPath(prefetchFile), (prefetchFile, false));
        }


        public FileStatus GetFileStatus(ulong hash, string name)
        {
            var match = FindFileHash(hash);

            if (match.Found && !match.Valid)
                return FileStatus.Invalid;
            if (match.Found)
                return FileStatus.Known;
            if (name != null && !KnownRootPaths.Any(name.StartsWith))
                return FileStatus.Suspect;

            return FileStatus.Known;
        }

        private (string Name, bool Valid, bool Found) FindFileHash(ulong hash)
        {
            if (CoreHashes.TryGetValue(hash, out var match))
                return (match.Name, match.Valid, true);
            if (StreamHashes.TryGetValue(hash, out match))
                return (match.Name, match.Valid, true);
            return (null, false, false);
        }

        public Mod LoadPath(string path)
        {
            foreach (var source in Sources)
            {
                var mod = source.TryLoad(path);
                if (mod != null)
                    return mod;
            }

            return null;
        }


    //public void AddFilesToPatch(Patch patch, IList<ModFile> files)
        //{
        //    var group =
        //        from f in files
        //        group f by new { f.SourcePath, f.SourceType } into g
        //        select new { g.Key.SourcePath, g.Key.SourceType, Files = g };

        //    foreach (var source in group)
        //    {
        //        switch (source.SourceType)
        //        {
        //            case SourceType.File:
        //                AddFilesFromDirectory(patch, source.Files);
        //                break;
        //            case SourceType.Zip:
        //                Sources[SourceType.Zip].AddFiles(patch, source.SourcePath, source.Files);
        //                break;
        //            case SourceType.Pack:
        //                break;
        //        }
        //    }

        //    if (files.Any())
        //    {
        //        var json = JsonConvert.SerializeObject(files, Formatting.Indented);
        //        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));

        //        patch.AddFile(ms, "CustomFiles.json", true);
        //    }
        //}

        //private void AddFilesFromDirectory(Patch patch, IEnumerable<ModFile> files)
        //{
        //    foreach (var file in files)
        //    {
        //        patch.AddFile(file.SourcePath, file.Name, true);
        //    }
        //}
    }
}
