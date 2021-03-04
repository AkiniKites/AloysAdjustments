﻿using System.Collections.Generic;
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
    public class DirSource : Source
    {
        public override SourceType SourceType => SourceType.Dir;

        public override Mod TryLoad(string path)
        {
            if (!Directory.Exists(path))
                return null;

            try
            {
                var dir = Path.GetFullPath(path);
                var filePaths = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

                var files = new List<ModFile>();
                foreach (var filePath in filePaths)
                {
                    var name = Files.Normalize(filePath.Substring(dir.Length + 1));

                    files.Add(new ModFile()
                    {
                        Name = name,
                        Path = name,
                        Status = GetFileStatus(Packfile.GetHashForPath(name), name)
                    });
                }

                return new Mod()
                {
                    Name = Path.GetFileName(path),
                    SourceType = SourceType,
                    Path = path,
                    Files = files.ToDictionary(x => x.Hash, x => x)
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