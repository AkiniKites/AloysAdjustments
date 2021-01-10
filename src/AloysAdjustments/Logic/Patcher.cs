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

        public async Task<Patch> StartPatch()
        {
            await FileManager.Cleanup(IoC.Config.TempPath);

            return new Patch(Path.Combine(IoC.Config.TempPath, PatchTempDir));
        }

        public async Task ApplyCustomPatches(Patch patch)
        {
            await Async.Run(() =>
            {
                Plugins.ExecuteAll<IPatchPlugin>(".", p =>
                {
                    p.ApplyChanges(patch);
                });
            });
        }

        public async Task PackPatch(Patch patch)
        {
            var output = Path.Combine(IoC.Config.TempPath, IoC.Config.PatchFile);

            await IoC.Archiver.PackFiles(patch.WorkingDir, output);

            patch.PackedFile = output;
        }

        public async Task InstallPatch(Patch patch)
        {
            if (!File.Exists(patch.PackedFile))
                throw new HzdException($"Patch file not found at: {patch.PackedFile}");
            if (!Directory.Exists(Configs.GamePackDir))
                throw new HzdException($"Pack directory not found at: {Configs.GamePackDir}");

            var dest = Path.Combine(Configs.GamePackDir, Path.GetFileName(patch.PackedFile));

            await Async.Run(() => File.Copy(patch.PackedFile, dest, true));
        }
    }
}
