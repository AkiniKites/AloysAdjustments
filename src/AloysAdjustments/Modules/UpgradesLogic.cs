using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Data;
using AloysAdjustments.Logic;
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
            //extract game files
            return await GenerateUpgradeListFromPath(Configs.GamePackDir, true);
        }

        public async Task<Dictionary<BaseGGUUID, Upgrade>> GenerateUpgradeListFromPath(string path, bool checkMissing)
        {
            var ignored = IoC.Config.IgnoredUpgrades.ToHashSet();

            var core = await IoC.Archiver.LoadFile(path, IoC.Config.UpgradeFile, checkMissing);

            if (core == null)
                return new Dictionary<BaseGGUUID, Upgrade>();

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

            return upgrades;
        }

        public async Task CreatePatch(string patchDir, List<Upgrade> upgrades)
        {
            //extract original outfit files to temp
            var core = await FileManager.ExtractFile(patchDir, 
                Configs.GamePackDir, IoC.Config.UpgradeFile);

            var upgradeChanges = upgrades.ToDictionary(x => x.Id, x => x);
            
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
