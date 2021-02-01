using System;
using System.Collections.Generic;
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
        public Dictionary<ulong, string> KnownHashes = new Dictionary<ulong, string>();
        public Dictionary<string, bool> KnownNames = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public void Initialize()
        {
            var prefetch = Prefetch.Load();

            foreach (var file in prefetch.Files.Keys)
            {
                KnownHashes.Add(Packfile.GetHashForPath(file), file);
                KnownNames.Add(file, true);
            }

            var prefetchFile = IoC.Config.PrefetchFile;
            KnownHashes.Add(Packfile.GetHashForPath(prefetchFile), prefetchFile);
            KnownNames.Add(prefetchFile, false);
        }

        public IEnumerable<CustomFile> GetFilesFromDir(string dir)
        {
            var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var name = Normalize(file.Substring(dir.Length + 1));
                KnownNames.TryGetValue(name, out var valid);
                yield return new CustomFile(name, file)
                {
                    Valid = valid
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
            var pack = new PackfileReader(path);

            var files = new List<CustomFile>();
            foreach (var file in pack.FileEntries)
            {
                var hasName = KnownHashes.TryGetValue(file.PathHash, out string name);
                var valid = false;
                if (hasName)
                    KnownNames.TryGetValue(name, out valid);
                files.Add(new CustomFile(hasName ? name : file.PathHash.ToString(), path)
                {
                    SourceType = SourceType.Pack,
                    Valid = valid
                });
            }

            return files;
        }


        private List<CustomFile> TryLoadAsZip(string path)
        {
            using var fs = File.OpenRead(path);
            var zip = new ZipArchive(fs, ZipArchiveMode.Read);

            var files = new List<CustomFile>();
            foreach (var entry in zip.Entries)
            {
                var name = Normalize(entry.FullName);
                KnownNames.TryGetValue(name, out var valid);
                files.Add(new CustomFile(name, path)
                {
                    Path = entry.FullName,
                    SourceType = SourceType.Zip,
                    Valid = valid
                });
            }

            return files;
        }

        private string Normalize(string path)
        {
            return path.Replace("\\", "/");
        }
    }
}
