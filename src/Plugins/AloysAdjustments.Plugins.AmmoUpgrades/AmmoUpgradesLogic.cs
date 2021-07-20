using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.AmmoUpgrades.Data;
using AloysAdjustments.Plugins.Upgrades;
using Decima;
using Decima.HZD;

namespace AloysAdjustments.Plugins.AmmoUpgrades
{
    public class AmmoUpgradesLogic
    {
        private readonly AmmoGenerator _ammoGenerator = new AmmoGenerator();

        public async Task<Dictionary<(BaseGGUUID, int), AmmoUpgrade>> GenerateUpgradeList(
            Func<string, Task<HzdCore>> coreGetter)
        {
            var ammoItems = _ammoGenerator.GetAmmoItems();
            var upgrades = new Dictionary<(BaseGGUUID, int), AmmoUpgrade>();
            
            await AddUpgradableAmmo(coreGetter, upgrades, ammoItems);
            await AddStackableAmmo(coreGetter, upgrades, ammoItems);
            
            return upgrades;
        }

        private async Task AddUpgradableAmmo(Func<string, Task<HzdCore>> coreGetter, 
            Dictionary<(BaseGGUUID, int), AmmoUpgrade> upgrades, List<AmmoItem> ammoItems)
        {
            var combined = IoC.Get<AmmoUpgradeConfig>().CombinedFiles;
            var upgradesCore = await IoC.Archiver.LoadGameFileAsync(IoC.Get<AmmoUpgradeConfig>().UpgradeFile);
            Dictionary<BaseGGUUID, RTTIRefObject> ammoUpgradesObjs = null;

            //get all recipes with facts
            var recipes = upgradesCore.GetTypesById<UpgradeRecipe>();
            foreach (var recipe in recipes)
            {
                //get item reference in entities/weapons/ammo/ammopouchupgrades
                var itemRef = recipe.Value.Item;
                ammoUpgradesObjs ??= (await IoC.Archiver.LoadGameFileAsync(itemRef.ExternalFile)).GetTypesById();

                var (name, fact, upgradeLevel) = GetUpgradeDetails(ammoUpgradesObjs, recipe.Value);
                if (name == null)
                    continue;

                async Task<AmmoUpgrade> CreateUpgrade(AmmoItem ammoItem, int level)
                {
                    int GetUpgradeValue(HzdCore core, int defaultValue)
                    {
                        if (core == null)
                            return defaultValue;

                        var obj = core.GetTypeById(ammoItem.StackableId);
                        return ((UpgradableStackableComponentResource)obj).UpgradedLimits[level];
                    }

                    var upgrade = new AmmoUpgrade()
                    {
                        Id = ammoItem.StackableId,
                        File = ammoItem.StackableFile,
                        Category = recipe.Value.Name,
                        LocalCategory = name,
                        Name = ammoItem.Name,
                        LocalName = ammoItem.LocalName,
                        Level = level,
                        Sort = 10,
                    };


                    upgrade.DefaultValue = GetUpgradeValue(await IoC.Archiver.LoadGameFileAsync(ammoItem.StackableFile), 0);
                    upgrade.Value = GetUpgradeValue(await coreGetter(ammoItem.StackableFile), upgrade.DefaultValue);

                    return upgrade;
                }

                void UpdateCombined(AmmoUpgrade upgrade)
                {
                    upgrade.Sort = 1;
                    upgrade.Name = upgrade.Category;
                    upgrade.LocalName = upgrade.LocalCategory;
                }

                foreach (var ammoItem in ammoItems.Where(x => x.FactId == fact.GUID && x.Upgradeable))
                {
                    var isCombined = combined.Contains(ammoItem.StackableFile);

                    var up = await CreateUpgrade(ammoItem, upgradeLevel);
                    if (isCombined) UpdateCombined(up);
                    upgrades.Add((up.Id, up.Level), up);

                    //level 1 upgrade, add a fake level 0 upgrade
                    if (upgradeLevel == 1)
                    {
                        var upZero = await CreateUpgrade(ammoItem, 0);
                        upgrades.Add((upZero.Id, upZero.Level), upZero);
                        if (isCombined) UpdateCombined(upZero);
                    }

                    //combined upgrade, only show 1
                    if (isCombined)
                        break;
                }
            }
        }

        private async Task AddStackableAmmo(Func<string, Task<HzdCore>> coreGetter,
            Dictionary<(BaseGGUUID, int), AmmoUpgrade> upgrades, List<AmmoItem> ammoItems)
        {
            foreach (var ammoItem in ammoItems.Where(x => !x.Upgradeable))
            {
                int GetUpgradeValue(HzdCore core, int defaultValue)
                {
                    if (core == null)
                        return defaultValue;

                    var obj = core.GetTypeById(ammoItem.StackableId);
                    return ((StackableComponentResource)obj).StackLimit;
                }

                var upgrade = new AmmoUpgrade()
                {
                    Id = ammoItem.StackableId,
                    File = ammoItem.StackableFile,
                    Category = ammoItem.Name,
                    LocalCategory = ammoItem.LocalName,
                    Name = ammoItem.Name,
                    LocalName = ammoItem.LocalName,
                    Level = 0,
                    Sort = 0,
                };


                upgrade.DefaultValue = GetUpgradeValue(await IoC.Archiver.LoadGameFileAsync(ammoItem.StackableFile), 0);
                upgrade.Value = GetUpgradeValue(await coreGetter(ammoItem.StackableFile), upgrade.DefaultValue);

                upgrades.Add((upgrade.Id, upgrade.Level), upgrade);
            }
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
        
        public void CreatePatch(Patch patch, List<AmmoUpgrade> upgrades)
        {
            foreach (var upgrade in upgrades.Where(x => x.Modified))
            {
                //extract original ammo files to temp
                var core = patch.AddFile(upgrade.File);
                
                var stackable = core.GetTypeById(upgrade.Id);

                if (stackable is UpgradableStackableComponentResource upgradeable)
                    upgradeable.UpgradedLimits[upgrade.Level] = upgrade.Value;
                else if (stackable is StackableComponentResource other)
                    other.StackLimit = upgrade.Value;

                core.Save();
            }
        }
    }
}
