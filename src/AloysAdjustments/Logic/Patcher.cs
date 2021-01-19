using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Modules;
using AloysAdjustments.Plugins;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Logic
{
    public class Patcher
    {
        private const string PatchTempDir = "patch";

        public PluginManager Plugins {get; }

        public Patcher()
        {
            Plugins = new PluginManager();
        }

        public Patch StartPatch()
        {
            FileManager.Cleanup(IoC.Config.TempPath);

            return new Patch(Path.Combine(IoC.Config.TempPath, PatchTempDir));
        }

        public void ApplyCustomPatches(Patch patch)
        {
            Plugins.ExecuteAll<IPatchPlugin>(".", p =>
            {
                p.ApplyChanges(patch);
            });
        }

        public void PackPatch(Patch patch)
        {
            var output = Path.Combine(IoC.Config.TempPath, IoC.Config.PatchFile);

            IoC.Archiver.PackFiles(patch.WorkingDir, patch.Files.Select(HzdCore.EnsureExt).ToArray(), output);

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
