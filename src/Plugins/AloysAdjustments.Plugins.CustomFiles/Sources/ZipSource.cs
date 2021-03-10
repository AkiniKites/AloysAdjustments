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
    public class ZipSource : Source
    {
        public override SourceType SourceType => SourceType.Zip;

        public override bool Validate(string path)
        {
            if (!File.Exists(path))
                return false;

            try
            {
                using var fs = File.OpenRead(path);
                using var zip = new ZipArchive(fs, ZipArchiveMode.Read);

                return zip.Entries.Any(x => Files.IsDir(x.FullName));
            }
            catch { }

            return false;
        }

        public override Mod TryLoad(string path)
        {
            if (!File.Exists(path))
                return null;

            try
            {
                using var fs = File.OpenRead(path);
                using var zip = new ZipArchive(fs, ZipArchiveMode.Read);

                var files = new List<ModFile>();
                foreach (var entry in zip.Entries)
                {
                    if (Files.IsDir(entry.FullName))
                        continue;

                    var name = Files.Normalize(entry.FullName);

                    var file = new ModFile()
                    {
                        Name = name,
                        Path = entry.FullName,
                        Hash = Packfile.GetHashForPath(name)
                    };
                    file.Status = GetFileStatus(file.Hash, name);

                    files.Add(file);
                }

                return new Mod()
                {
                    Name = Path.GetFileNameWithoutExtension(path),
                    SourceType = SourceType,
                    Path = path,
                    Files = files.ToDictionary(x => x.Hash, x => x),
                    FileStatus = files.Any(x => x.Status == FileStatus.Suspect) ? 
                        ModFileStatus.Suspect : ModFileStatus.Normal
                };
            }
            catch { }

            return null;
        }

        //public override void AddFiles(Patch patch, string source, IEnumerable<ModFile> files)
        //{
        //    if (!File.Exists(source))
        //        throw new PatchException($"Zip file not found: {source}");

        //    using var fs = File.OpenRead(source);
        //    var zip = new ZipArchive(fs, ZipArchiveMode.Read);

        //    foreach (var file in files)
        //    {
        //        var entry = zip.Entries.FirstOrDefault(x => x.FullName == file.SubPath);
        //        if (entry == null)
        //            throw new PatchException($"File '{file.SubPath}' not found in zip: {source}");

        //        using var zs = entry.Open();
        //        patch.AddFile(zs, file.Name, true);
        //    }
        //}
    }
}
