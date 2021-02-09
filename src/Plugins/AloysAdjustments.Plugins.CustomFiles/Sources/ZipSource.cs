using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.CustomFiles.Utility;
using Decima;

namespace AloysAdjustments.Plugins.CustomFiles.Sources
{
    public class ZipSource : FileSource
    {
        public override SourceType SourceType => SourceType.Zip;

        public override List<CustomFile> TryLoad(string path)
        {
            try
            {
                using var fs = File.OpenRead(path);
                var zip = new ZipArchive(fs, ZipArchiveMode.Read);

                var files = new List<CustomFile>();
                foreach (var entry in zip.Entries)
                {
                    if (Files.IsDir(entry.FullName))
                        continue;

                    var name = Files.Normalize(entry.FullName);

                    var file = new CustomFile()
                    {
                        Name = name,
                        SubPath = entry.FullName,
                        SourcePath = path,
                        SourceType = SourceType.Zip,
                        Status = GetFileStatus(Packfile.GetHashForPath(name), name)
                    };
                    files.Add(file);
                }

                return files;
            }
            catch { }

            return null;
        }

        public override void AddFiles(Patch patch, string source, IEnumerable<CustomFile> files)
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
    }

    public abstract class FileSource
    {
        public Func<ulong, string, FileStatus>  GetFileStatus { get; set; }

        public abstract SourceType SourceType { get; }
        public abstract List<CustomFile> TryLoad(string path);
        public abstract void AddFiles(Patch patch, string source, IEnumerable<CustomFile> files);
    }
}
