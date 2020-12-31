using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Data;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;
using Upgrade = AloysAdjustments.Data.Upgrade;

namespace AloysAdjustments.Modules
{
    public class UpgradesLogic
    {
        public async Task<Dictionary<BaseGGUUID, Upgrade>> GenerateUpgradeList()
        {
            //TODO: Fix hack to ignore patch files
            var patch = Path.Combine(Configs.GamePackDir, IoC.Config.PatchFile);
            using var rn = new FileRenamer(patch);

            //extract game files
            var upgrades = await GenerateUpgradeListFromPath(Configs.GamePackDir);
            return upgrades;
        }

        public async Task<Dictionary<BaseGGUUID, Upgrade>> GenerateUpgradeListFromPath(string path)
        {
            var ignored = IoC.Config.IgnoredUpgrades.ToHashSet();

            var file = await FileManager.ExtractFile(IoC.Decima,
                IoC.Config.TempPath, path, false, IoC.Config.UpgradeFile);
            
            var core = HzdCore.Load(file.Output);

            var charUpgrades = core.GetTypes<CharacterUpgrade>();
            var invMods = core.GetTypes<InventoryCapacityModificationComponentResource>();
            
            var upgrades = new Dictionary<BaseGGUUID, Upgrade>();
            foreach (var charUpgrade in charUpgrades.Values)
            {
                var modRef = charUpgrade.Components.FirstOrDefault();

                if (modRef?.GUID == null || !invMods.TryGetValue(modRef.GUID, out var invMod))
                    continue;

                if (ignored.Contains(invMod.ObjectUUID.ToString()))
                    continue;

                //TODO: maybe allow individual changes
                var value = new[] {
                    invMod.WeaponsCapacityIncrease,
                    invMod.ModificationsCapacityIncrease,
                    invMod.OutfitsCapacityIncrease,
                    invMod.ResourcesCapacityIncrease,
                    invMod.ToolsCapacityIncrease
                }.Max();

                var up = new Upgrade
                {
                    Id = invMod.ObjectUUID,
                    Name = invMod.Name,
                    Value = value,
                    LocalNameId = charUpgrade.DisplayName.GUID,
                    LocalNameFile = charUpgrade.DisplayName.ExternalFile.ToString()
                };

                upgrades.Add(up.Id, up);
            }

            await FileManager.Cleanup(IoC.Config.TempPath);

            return upgrades;
        }

        public async Task CreatePatch(string patchDir, List<Upgrade> upgrades)
        {
            //extract original outfit files to temp
            var file = await FileManager.ExtractFile(
                IoC.Decima, patchDir, Configs.GamePackDir, true, IoC.Config.UpgradeFile);

            var upgradeChanges = upgrades.ToDictionary(x => x.Id, x => x);

            var core = HzdCore.Load(file.Output);
            var invMods = core.GetTypes<InventoryCapacityModificationComponentResource>();

            foreach (var invMod in invMods.Values)
            {
                if (!upgradeChanges.TryGetValue(invMod.ObjectUUID, out var upgrade))
                    continue;

                if (invMod.WeaponsCapacityIncrease > 0) invMod.WeaponsCapacityIncrease = upgrade.Value;
                else if(invMod.ModificationsCapacityIncrease > 0) invMod.ModificationsCapacityIncrease = upgrade.Value;
                else if (invMod.OutfitsCapacityIncrease > 0) invMod.OutfitsCapacityIncrease = upgrade.Value;
                else if (invMod.ResourcesCapacityIncrease > 0) invMod.ResourcesCapacityIncrease = upgrade.Value;
                else if (invMod.ToolsCapacityIncrease > 0) invMod.ToolsCapacityIncrease = upgrade.Value;
            }

            core.Save();
        }
    }
}
