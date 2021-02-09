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
    public class CustomFilesLogic
    {
        private readonly HashSet<string> KnownRootPaths = new HashSet<string>();
        private readonly Dictionary<ulong, (string Name, bool Valid)> CoreHashes 
            = new Dictionary<ulong, (string Name, bool Valid)>();
        //mostly guesses
        private readonly Dictionary<ulong, (string Name, bool Valid)> StreamHashes 
            = new Dictionary<ulong, (string Name, bool Valid)>();

        private readonly Dictionary<SourceType, FileSource> Sources;

        public CustomFilesLogic()
        {
            Sources = new Dictionary<SourceType, FileSource> {
                { SourceType.Zip, new ZipSource() }
            };

            foreach (var source in Sources)
            {
                source.Value.GetFileStatus = GetFileStatus;
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
            var match = FindFile(hash);

            if (match.Found && !match.Valid)
                return FileStatus.Invalid;
            if (match.Found)
                return FileStatus.Known;
            if (name != null && KnownRootPaths.Any(name.StartsWith))
                return FileStatus.Suspect;

            return FileStatus.Known;
        }

        private (string Name, bool Valid, bool Found) FindFile(ulong hash)
        {
            if (CoreHashes.TryGetValue(hash, out var match))
                return (match.Name, match.Valid, true);
            if (StreamHashes.TryGetValue(hash, out match))
                return (match.Name, match.Valid, true);
            return (null, false, false);
        }

        public IEnumerable<CustomFile> GetFilesFromDir(string dir)
        {
            var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var name = Files.Normalize(file.Substring(dir.Length + 1));

                yield return new CustomFile()
                {
                    Name = name,
                    SourcePath = file,
                    SourceType = SourceType.File,
                    Status = GetFileStatus(Packfile.GetHashForPath(name), name)
                };
            }
        }

        public IEnumerable<CustomFile> GetFiles(string sourceFile)
        {
            var packFiles = TryLoadAsPack(sourceFile);
            if (packFiles != null) return packFiles;

            var zipFiles = Sources[SourceType.Zip].TryLoad(sourceFile);
            if (zipFiles != null) return zipFiles;

            return new List<CustomFile>();
        }
        private List<CustomFile> TryLoadAsPack(string path)
        {
            int unknown = 0;
            try
            {
                var pack = new PackfileReader(path);

                var files = new List<CustomFile>();
                foreach (var file in pack.FileEntries)
                {
                    var known = FindFile(file.PathHash);
                    files.Add(new CustomFile()
                    {
                        Name = known.Found ? known.Name + Packfile.CoreExt : $"Unknown file name - {file.PathHash}",
                        SourcePath = path,
                        SourceType = SourceType.Pack,
                        Status = GetFileStatus(file.PathHash, null)
                    });
                }

                Debug.WriteLine(unknown);
                return files;
            }
            catch { }

            return null;
        }
        
        public void AddFilesToPatch(Patch patch, IList<CustomFile> files)
        {
            var group =
                from f in files
                group f by new { f.SourcePath, f.SourceType } into g
                select new { g.Key.SourcePath, g.Key.SourceType, Files = g };

            foreach (var source in group)
            {
                switch (source.SourceType)
                {
                    case SourceType.File:
                        AddFilesFromDirectory(patch, source.Files);
                        break;
                    case SourceType.Zip:
                        Sources[SourceType.Zip].AddFiles(patch, source.SourcePath, source.Files);
                        break;
                    case SourceType.Pack:
                        break;
                }
            }

            if (files.Any())
            {
                var json = JsonConvert.SerializeObject(files, Formatting.Indented);
                using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));

                patch.AddFile(ms, "CustomFiles.json", true);
            }
        }

        private void AddFilesFromDirectory(Patch patch, IEnumerable<CustomFile> files)
        {
            foreach (var file in files)
            {
                patch.AddFile(file.SourcePath, file.Name, true);
            }
        }
    }
}
