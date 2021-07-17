using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.Upgrades.Data;
using Decima;
using Decima.HZD;
using Upgrade = AloysAdjustments.Plugins.Upgrades.Data.Upgrade;

namespace AloysAdjustments.Plugins.Upgrades
{
    public class AmmoUpgradesLogic
    {
        public async Task<Dictionary<BaseGGUUID, Upgrade>> GenerateUpgradeList(
            Func<string, Task<HzdCore>> coreGetter)
        {
            var ammoItems = new AmmoGenerator().GetAmmoItems();
            var upgradesCore = await IoC.Archiver.LoadGameFileAsync(IoC.Get<UpgradeConfig>().UpgradeFile);
            Dictionary<BaseGGUUID, RTTIRefObject> ammoUpgradesObjs = null;

            var upgrades = new Dictionary<BaseGGUUID, Upgrade>();

            //get all recipes with facts
            var recipes = upgradesCore.GetTypesById<UpgradeRecipe>();
            foreach (var recipe in recipes)
            {
                //get item reference in entities/weapons/ammo/ammopouchupgrades
                var itemRef = recipe.Value.Item;
                ammoUpgradesObjs ??= (await IoC.Archiver.LoadGameFileAsync(itemRef.ExternalFile)).GetTypesById();

                var (name, fact, level) = GetUpgradeDetails(ammoUpgradesObjs, recipe.Value);
                if (name == null)
                    continue;

                var ammoItem = ammoItems.FirstOrDefault(x => x.FactId == fact.GUID);
                if (ammoItem == null)
                    continue;
                
                int GetUpgradeValue(HzdCore core, int defaultValue)
                {
                    if (core == null)
                        return defaultValue;
                    var stackable = (UpgradableStackableComponentResource)core.GetTypeById(ammoItem.Id);
                    return stackable.UpgradedLimits[level];
                }
                
                var upgrade = new Upgrade()
                {
                    Id = recipe.Key,
                    Name = recipe.Value.Name,
                    Type = UpgradeType.Ammo,
                    Level = level,
                    LocalNameFile = name.ExternalFile,
                    LocalNameId = name.GUID,
                };

                upgrade.DefaultValue = GetUpgradeValue(await IoC.Archiver.LoadGameFileAsync(ammoItem.File), 0);
                upgrade.Value = GetUpgradeValue(await coreGetter(ammoItem.File), upgrade.DefaultValue);

                upgrades.Add(upgrade.Id, upgrade);
            }

            return upgrades;
        }
        
        private (Ref<LocalizedTextResource>, Ref<BaseResource>, int) GetUpgradeDetails(
            Dictionary<BaseGGUUID, RTTIRefObject> objs, UpgradeRecipe recipe)
        {
            Ref<LocalizedTextResource> name = null;
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
                    name = item.LocalizedItemName;
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
