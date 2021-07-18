using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using Decima;
using Decima.HZD;
using Upgrade = AloysAdjustments.Plugins.Upgrades.Data.Upgrade;

namespace AloysAdjustments.Plugins.Upgrades
{
    public class CharacterUpgradesLogic
    {
        public async Task<Dictionary<BaseGGUUID, Upgrade>> GenerateUpgradeList(
            Func<string, Task<HzdCore>> coreGetter)
        {
            var ignored = IoC.Get<UpgradeConfig>().IgnoredUpgrades.ToHashSet();

            var core = await coreGetter(IoC.Get<UpgradeConfig>().UpgradeFile);

            if (core == null)
                return new Dictionary<BaseGGUUID, Upgrade>();

            var charUpgrades = core.GetTypesById<CharacterUpgrade>();
            var invMods = core.GetTypesById<InventoryCapacityModificationComponentResource>();
            var sets = core.GetTypesById<CharacterUpgradeSet>();
            
            var upgrades = new Dictionary<BaseGGUUID, Upgrade>();
            foreach (var charUpgrade in charUpgrades.Values)
            {
                var modRef = charUpgrade.Components.FirstOrDefault();

                if (modRef?.GUID == null || !invMods.TryGetValue(modRef.GUID, out var invMod))
                    continue;

                if (ignored.Contains(invMod.ObjectUUID.ToString()))
                    continue;
                
                var value = new[] {
                    invMod.WeaponsCapacityIncrease,
                    invMod.ModificationsCapacityIncrease,
                    invMod.OutfitsCapacityIncrease,
                    invMod.ResourcesCapacityIncrease,
                    invMod.ToolsCapacityIncrease
                }.Max();
                
                var upgrade = new Upgrade
                {
                    Id = invMod.ObjectUUID,
                    Name = invMod.Name,
                    LocalName = charUpgrade.DisplayName,
                    Value = value,
                    DefaultValue = value,
                };
                
                upgrades.Add(upgrade.Id, upgrade);
            }

            return upgrades;
        }

        public void CreatePatch(Patch patch, List<Upgrade> upgrades)
        {
            //extract original outfit files to temp
            var core = patch.AddFile(IoC.Get<UpgradeConfig>().UpgradeFile);

            var upgradeChanges = upgrades.ToDictionary(x => x.Id, x => x);
            
            var invMods = core.GetTypes<InventoryCapacityModificationComponentResource>();

            foreach (var invMod in invMods)
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
