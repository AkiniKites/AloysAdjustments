using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using Decima;

namespace AloysAdjustments.Plugins.CustomFiles
{
    public class CustomFilesLogic
    {
        public Dictionary<ulong, (string Name, bool Valid)> KnownCores = new Dictionary<ulong, (string Name, bool Valid)>();
        //mostly guesses
        public Dictionary<ulong, (string Name, bool Valid)> KnownStreams = new Dictionary<ulong, (string Name, bool Valid)>();

        public void Initialize()
        {
            var prefetch = Prefetch.Load();

            foreach (var file in prefetch.Files.Keys)
            {
                KnownCores.Add(Packfile.GetHashForPath(file), (file, true));
                KnownStreams.Add(Packfile.GetHashForPath(file, true), (file, true));
            }

            //prefetch not valid, must be generated
            var prefetchFile = IoC.Config.PrefetchFile;
            KnownCores.Add(Packfile.GetHashForPath(prefetchFile), (prefetchFile, false));
        }

        private (string Name, bool Valid, bool Found) FindFile(ulong hash)
        {
            if (KnownCores.TryGetValue(hash, out var knownCore))
                return (knownCore.Name, knownCore.Valid, true);
            return (null, false, false);
        }

        public IEnumerable<CustomFile> GetFilesFromDir(string dir)
        {
            var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var name = Normalize(file.Substring(dir.Length + 1));
                var known = FindFile(Packfile.GetHashForPath(name));

                yield return new CustomFile()
                {
                    Name = name,
                    Valid = !known.Found || known.Valid,
                    SourcePath = file,
                    SourceType = SourceType.File,
                };
            }
        }

        public IEnumerable<CustomFile> GetFiles(string sourceFile)
        {
            var packFiles = TryLoadAsPack(sourceFile);
            if (packFiles != null) return packFiles;

            var zipFiles = TryLoadAsZip(sourceFile);
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
                    if (known.Name == null)
                        unknown++;

                    files.Add(new CustomFile()
                    {
                        Name = known.Found ? known.Name + Packfile.CoreExt : $"Unknown file name - {file.PathHash}",
                        Valid = !known.Found || known.Valid,
                        SourcePath = path,
                        SourceType = SourceType.Pack,
                    });
                }

                Debug.WriteLine(unknown);
                return files;
            }
            catch { }

            return null;
        }


        private List<CustomFile> TryLoadAsZip(string path)
        {
            try
            {
                using var fs = File.OpenRead(path);
                var zip = new ZipArchive(fs, ZipArchiveMode.Read);

                var files = new List<CustomFile>();
                foreach (var entry in zip.Entries)
                {
                    if (IsDir(entry.FullName))
                        continue;

                    var name = Normalize(entry.FullName);
                    var known = FindFile(Packfile.GetHashForPath(name));
                    files.Add(new CustomFile()
                    {
                        Name = name,
                        Valid = !known.Found || known.Valid,
                        SubPath = entry.FullName,
                        SourcePath = path,
                        SourceType = SourceType.Zip,
                    });
                }

                return files;
            }
            catch { }

            return null;
        }

        public void AddFilesToPatch(Patch patch, IEnumerable<CustomFile> files)
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
                        AddFilesFromZip(patch, source.SourcePath, source.Files);
                        break;
                    case SourceType.Pack:
                        break;
                    default:
                        break;
                }
            }
        }

        private void AddFilesFromDirectory(Patch patch, IEnumerable<CustomFile> files)
        {
            foreach (var file in files)
            {
                patch.AddFile(file.SourcePath, file.Name, true);
            }
        }

        private void AddFilesFromZip(Patch patch, string source, IEnumerable<CustomFile> files)
        {
            if (!File.Exists(source))
                throw new PatchException($"Zip file not found: {source}");

            using var fs = File.OpenRead(source);
            var zip = new ZipArchive(fs, ZipArchiveMode.Read);

            foreach (var file in files)
            {
                var entry = zip.Entries.FirstOrDefault(x => x.FullName == file.SubPath);
                if (entry == null)
                    throw new PatchException($"File '{file.SubPath}' not found in zip: {source}");

                using var zs = entry.Open();
                patch.AddFile(zs, file.Name, true);
            }
        }

        private string Normalize(string path)
        {
            return path.Replace("\\", "/");
        }

        private bool IsDir(string path)
        {
            if (path == null)
                return false;

            return path.Last() == Path.DirectorySeparatorChar ||
                path.Last() == Path.AltDirectorySeparatorChar;
        }
    }
}
