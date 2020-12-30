using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Data;
using Decima;
using Decima.HZD;

namespace AloysAdjustments
{
    public class Patcher
    {
        private const string PatchTempDir = "patch";

        public async Task<string> SetupPatchDir()
        {
            await FileManager.Cleanup(IoC.Config.TempPath);

            return Path.Combine(IoC.Config.TempPath, PatchTempDir);
        }
        
        public async Task<string> PackPatch(string patchDir)
        {
            await UpgradeMods(patchDir);

            var output = Path.Combine(IoC.Config.TempPath, IoC.Config.PatchFile);

            await IoC.Decima.PackFiles(patchDir, output);

            return output;
        }

        private async Task UpgradeMods(string patchDir)
        {
            var upgrades = await FileManager.ExtractFile(IoC.Decima, patchDir, Configs.GamePackDir,
                true, "entities/characters/craftingrecipes.core");

            var core = HzdCore.Load(upgrades.Output);
            var upComps = core.GetTypes<InventoryCapacityModificationComponentResource>();
            var lastRes = upComps.FirstOrDefault(x => x.Value.ResourcesCapacityIncrease >= 80);
            lastRes.Value.ResourcesCapacityIncrease = 800;

            core.Save();
        }

        public async Task InstallPatch(string path)
        {
            await FileManager.InstallPatch(path, Configs.GamePackDir);
        }
    }
}
