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
    public class AmmoUpgradesLogic
    {
        public async Task<Dictionary<BaseGGUUID, Upgrade>> GenerateAmmoList(
            Func<string, Task<HzdCore>> coreGetter)
        {
            new AmmoGenerator().GetCharacterModels();
            return null;
        }

        public async Task<Dictionary<BaseGGUUID, Upgrade>> GenerateUpgradeList(
            Func<string, Task<HzdCore>> coreGetter)
        {
            var ignored = IoC.Get<UpgradeConfig>().IgnoredUpgrades.ToHashSet();

            await GenerateUpgrades(ignored);

            return null;
        }
        
        private async Task GenerateUpgrades(System.Collections.Generic.HashSet<string> ignored)
        {
            var upgradesCore = await IoC.Archiver.LoadGameFileAsync(IoC.Get<UpgradeConfig>().UpgradeFile);
            Dictionary<BaseGGUUID, RTTIRefObject> ammoUpgradesObjs = null;

            //get all recipes
            var recipes = upgradesCore.GetTypesById<UpgradeRecipe>();
            foreach (var recipe in recipes)
            {
                //get item reference in entities/weapons/ammo/ammopouchupgrades
                var itemRef = recipe.Value.Item;
                if (ammoUpgradesObjs == null)
                    ammoUpgradesObjs = (await IoC.Archiver.LoadGameFileAsync(itemRef.ExternalFile)).GetTypesById();

            }

            return ;


            //var invMods = core.GetTypesById<InventoryCapacityModificationComponentResource>();

            //var upgrades = new Dictionary<BaseGGUUID, Upgrade>();
            //foreach (var charUpgrade in charUpgrades.Values)
            //{
            //    var modRef = charUpgrade.Components.FirstOrDefault();

            //    if (modRef?.GUID == null || !invMods.TryGetValue(modRef.GUID, out var invMod))
            //        continue;

            //    if (ignored.Contains(invMod.ObjectUUID.ToString()))
            //        continue;

            //    //TODO: maybe allow individual changes
            //    var value = new[] {
            //        invMod.WeaponsCapacityIncrease,
            //        invMod.ModificationsCapacityIncrease,
            //        invMod.OutfitsCapacityIncrease,
            //        invMod.ResourcesCapacityIncrease,
            //        invMod.ToolsCapacityIncrease
            //    }.Max();

            //    var up = new Upgrade
            //    {
            //        Id = invMod.ObjectUUID,
            //        Name = invMod.Name,
            //        Value = value,
            //        DefaultValue = value,
            //        LocalNameId = charUpgrade.DisplayName.GUID,
            //        LocalNameFile = charUpgrade.DisplayName.ExternalFile.ToString()
            //    };

            //    upgrades.Add(up.Id, up);
            //}

            //return upgrades;
        }

        private async Task<(string, Ref<BaseResource>, int)> GetUpgradeDetails(
            Dictionary<BaseGGUUID, RTTIRefObject> objs, UpgradeRecipe recipe)
        {
            string name = null;
            Ref<BaseResource> fact = null;
            int upgradeLevel = 0;

            //get reference object for upgrade
            var entity = (EntityResource)objs[recipe.Item.GUID];
            foreach (var componentRef in entity.EntityComponentResources)
            {
                var comp = objs[componentRef.GUID];
                if (comp is InventoryItemComponentResource item)
                {
                    //component has upgrade name
                    name = await IoC.Localization.GetString(item.LocalizedItemName.ExternalFile, item.LocalizedItemName.GUID);
                }
                else if (comp is InventoryNodeGraphPackageComponentResource node)
                {
                    var unpack = (OverrideGraphProgramResource)objs[node.UnpackOverrideGraphProgram.GUID];
                    foreach (var variableRef in unpack.VariableOverrides)
                    {
                        //one program override has the fact name other has the value
                        var variable = objs[variableRef.GUID];
                        if (variable is NodeGraphResourceVariableOverride nodeGraph)
                            fact = nodeGraph.Object;
                        else if (variable is NodeGraphIntVariableOverride val)
                            upgradeLevel = val.Value;
                    }
                }
            }

            return (name, fact, upgradeLevel);
        }
    }
}
