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
    public class PakSource : Source
    {
        public override SourceType SourceType => SourceType.Pack;

        public override bool Validate(string path)
        {
            if (!File.Exists(path))
                return false;
            try
            {
                var pack = new PackfileReader(path);
                return pack.FileEntries.Any();
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
                var pack = new PackfileReader(path);

                var files = new List<ModFile>();
                foreach (var file in pack.FileEntries)
                {
                    var status = GetFileStatus(file.PathHash, null);
                    var name = status == FileStatus.Known ? GetFileName(file.PathHash) + Packfile.CoreExt : $"Unknown file name - {file.PathHash}";

                    files.Add(new ModFile()
                    {
                        Name = name,
                        Hash = file.PathHash,
                        Status = status
                    });
                }
                
                return new Mod()
                {
                    Name = Path.GetFileNameWithoutExtension(path),
                    SourceType = SourceType,
                    Path = path,
                    Files = files.ToDictionary(x => x.Hash, x => x)
                };
            }
            catch { }

            return null;
        }
    }
}
