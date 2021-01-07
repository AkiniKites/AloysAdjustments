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

        public async Task<string> SetupPatchDir()
        {
            await FileManager.Cleanup(IoC.Config.TempPath);

            return Path.Combine(IoC.Config.TempPath, PatchTempDir);
        }

        public async Task ApplyCustomPatches(string patchDir)
        {
            await Async.Run(() =>
            {
                Plugins.ExecuteAll<IPlugin>(".", p =>
                {
                    p.CreatePatch(patchDir);
                });
            });
        }

        public async Task<string> PackPatch(string patchDir)
        {
            var output = Path.Combine(IoC.Config.TempPath, IoC.Config.PatchFile);

            await IoC.Archiver.PackFiles(patchDir, output);

            return output;
        }

        public async Task InstallPatch(string path)
        {
            await FileManager.InstallPatch(path, Configs.GamePackDir);
        }
    }
}
