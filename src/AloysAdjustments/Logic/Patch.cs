using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Logic
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

        public HzdCore AddFile(string file)
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
    }
}
