using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Plugins;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Logic.Patching
{
    public class Patcher
    {
        private const string PatchTempDir = "patch";
        
        public Patcher() { }

        public Patch StartPatch()
        {
            Paths.Cleanup(IoC.Config.TempPath);

            return new Patch(Path.Combine(IoC.Config.TempPath, PatchTempDir));
        }

        public void ApplyCustomPatches(Patch patch, PluginManager plugins)
        {
            foreach (var plugin in plugins.LoadAll<IPlugin>())
            {
                plugin.ApplyChanges(patch);
            }
        }

        public void PackPatch(Patch patch)
        {
            var output = Path.Combine(IoC.Config.TempPath, IoC.Config.PatchFile);

            IoC.Archiver.PackFiles(patch.WorkingDir, patch.Files.ToArray(), output);

            patch.PackedFile = output;
        }

        public void InstallPatch(Patch patch)
        {
            if (!File.Exists(patch.PackedFile))
                throw new HzdException($"Patch file not found at: {patch.PackedFile}");
            if (!Directory.Exists(Configs.GamePackDir))
                throw new HzdException($"Pack directory not found at: {Configs.GamePackDir}");

            var dest = Path.Combine(Configs.GamePackDir, Path.GetFileName(patch.PackedFile));
            
            File.Copy(patch.PackedFile, dest, true);
        }
    }
}
