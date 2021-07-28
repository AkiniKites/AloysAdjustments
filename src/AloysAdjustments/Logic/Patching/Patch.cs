using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AloysAdjustments.Common.Utility;
using AloysAdjustments.Utility;
using Newtonsoft.Json;

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
                Patch.Files.Add(HzdCore.EnsureExt(Source));

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

        public HzdCore AddFile(string file)
        {
            var subPath = NormalizeSubPath(HzdCore.EnsureExt(file));
            var path = Path.Combine(WorkingDir, subPath);

            var core = Files.Contains(subPath) ? 
                HzdCore.FromFile(path, subPath) : 
                IoC.Archiver.LoadGameFile(subPath);

            var patchCore = new HzdCorePatch(core)
            {
                Patch = this,
                FilePath = path
            };

            return patchCore;
        }

        public void AddFile(string source, string filePath, bool overwrite = false)
        {
            var subPath = NormalizeSubPath(filePath);
            var path = Path.Combine(WorkingDir, subPath);

            if (!File.Exists(source))
                throw new PatchException($"File not found, cannot add to pack: {source}");
            if (Files.Contains(subPath) && !overwrite)
                throw new PatchException($"File already exists in pack: {subPath}");

            Paths.CheckDirectory(Path.GetDirectoryName(path));
            File.Copy(source, path, true);

            Files.Add(subPath);
        }

        public void AddFile(Stream source, string filePath, bool overwrite = false)
        {
            var subPath = NormalizeSubPath(filePath);
            var path = Path.Combine(WorkingDir, subPath);

            if (Files.Contains(subPath) && !overwrite)
                throw new PatchException($"Attempted to add duplicate file to patch: {subPath}");

            Paths.CheckDirectory(Path.GetDirectoryName(path));
            using var fs = File.Create(path);
            source.CopyTo(fs);

            Files.Add(subPath);
        }

        private string NormalizeSubPath(string path)
        {
            return path.Replace("\\", "/");
        }
    }
}
