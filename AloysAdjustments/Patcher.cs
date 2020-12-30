using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;
using Decima.HZD;
using AloysAdjustments.Models;

namespace AloysAdjustments
{
    public class Patcher
    {
        private const string PatchTempDir = "patch";

        public async Task<string> GeneratePatch(OutfitFile[] maps)
        {
            await FileManager.Cleanup(IoC.Config.TempPath);

            var patchDir = Path.Combine(IoC.Config.TempPath, PatchTempDir);

            foreach (var map in maps)
            {
                //extract original outfit files to temp
                var file = await FileManager.ExtractFile(
                    IoC.Decima, patchDir, Configs.GamePackDir, true, map.File);

                var refs = map.Outfits.ToDictionary(x => x.RefId, x => x.ModelId);

                //update references from based on new maps
                var core = HzdCore.Load(file.Output);
                foreach (var reference in core.GetTypes<NodeGraphHumanoidBodyVariantUUIDRefVariableOverride>().Values)
                {
                    if (refs.TryGetValue(reference.ObjectUUID, out var newModel))
                        reference.Object.GUID.AssignFromOther(newModel);
                }

                core.Save();
            }

            //await AddCharacterReferences(patchDir);
            await UpgradeMods(patchDir);

            var output = Path.Combine(IoC.Config.TempPath, IoC.Config.PatchFile);

            await IoC.Decima.PackFiles(patchDir, output);

            return output;

            //await FileManager.Cleanup(IoC.Config.TempPath);
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
