using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Logic.Patching
{
    public class Patch
    {
        private class HzdCorePatch : HzdCore
        {
            public Patch Patch { get; set; }
            public string FilePath { get; set; }

            public HzdCorePatch(HzdCore core)
            {
                Source = core.Source;
                Binary = core.Binary;
            }

            public override void Save()
            {
                Patch.Files.Add(Source);

                Paths.CheckDirectory(Path.GetDirectoryName(FilePath));
                Binary.ToFile(FilePath);
            }
        }

        public string WorkingDir { get; }
        public string PackedFile { get; set; }
        public HashSet<string> Files { get; }

        public Patch(string workingDir)
        {
            WorkingDir = workingDir;
            Files = new HashSet<string>();
        }

        public HzdCore AddGameFile(string file)
        {
            file = HzdCore.NormalizeSource(file);
            var path = Path.Combine(WorkingDir, HzdCore.EnsureExt(file));

            var core = Files.Contains(file) ? 
                HzdCore.FromFile(path, file) : 
                IoC.Archiver.LoadGameFile(file);

            var patchCore = new HzdCorePatch(core)
            {
                Patch = this,
                FilePath = path
            };

            return patchCore;
        }

        public void AddFile(string source, string filePath, bool overwrite = false)
        {
            filePath = HzdCore.NormalizeSource(filePath);
            var path = Path.Combine(WorkingDir, filePath);

            if (!File.Exists(source))
                throw new PatchException($"File not found, cannot add to pack: {source}");
            if (Files.Contains(filePath) && !overwrite)
                throw new PatchException($"File already exists in pack: {filePath}");

            Paths.CheckDirectory(Path.GetDirectoryName(path));
            File.Copy(source, path, true);

            Files.Add(filePath);
        }

        public void AddFile(Stream source, string filePath, bool overwrite = false)
        {
            filePath = HzdCore.NormalizeSource(filePath);
            var path = Path.Combine(WorkingDir, filePath);

            if (Files.Contains(filePath) && !overwrite)
                throw new PatchException($"Attempted to add duplicate file to patch: {filePath}");

            Paths.CheckDirectory(Path.GetDirectoryName(path));
            using var fs = File.Create(path);
            source.CopyTo(fs);

            Files.Add(filePath);
        }
    }
}
