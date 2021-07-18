using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Plugins.Upgrades.Data;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;

namespace AloysAdjustments.Plugins.Upgrades
{
    public class AmmoGenerator
    {
        private readonly Regex AmmoMatcher;

        private readonly FileCollector<AmmoItem> _ammoFileCollector;

        public AmmoGenerator()
        {
            //HumanoidMatcher = new Regex(IoC.Get<UpgradeConfig>().HumanoidMatcher);
            AmmoMatcher = new Regex("^entities/.*weapons/ammo/.+");
            //Ignored = IoC.Get<UpgradeConfig>().IgnoredCharacters.ToArray();
            var ignored = new[] { "aiammo" };

            _ammoFileCollector = new FileCollector<AmmoItem>("ammo",
                IsAmmoFile, GetAmmoItems, ignored);
        }

        public List<AmmoItem> GetAmmoItems()
        {
            return _ammoFileCollector.Load();
        }

        private bool IsAmmoFile(string file)
        {
            return AmmoMatcher.IsMatch(file);
        }
        
        private IEnumerable<AmmoItem> GetAmmoItems(string file)
        {
            var pack = IoC.Archiver.LoadGameFile(file);
            var projectiles = pack.GetTypes<EntityProjectileAmmoResource>();
            
            foreach (var projectile in projectiles)
            {
                var ammo = new AmmoItem()
                {
                    Name = projectile.Name,
                    MainId = projectile.ObjectUUID,
                    MainFile = file
                };

                foreach (var comp in projectile.EntityComponentResources)
                {
                    if (comp.Type == BaseRef.Types.LocalCoreUUID)
                    {
                        if (pack.GetTypeById(comp.GUID) is InventoryItemComponentResource item)
                            ammo.LocalName = item.LocalizedItemName;
                    }
                    else
                    {
                        var core = IoC.Archiver.LoadGameFile(comp.ExternalFile);
                        if (core.GetTypeById(comp.GUID) is UpgradableStackableComponentResource stackable)
                        {
                            ammo.UpgradeId = stackable.ObjectUUID;
                            ammo.UpgradeFile = comp.ExternalFile;
                            ammo.FactId = stackable.UpgradeLevelFact.GUID;
                            ammo.FactFile = stackable.UpgradeLevelFact.ExternalFile;
                        }
                    }
                }

                if (ammo.UpgradeId != null)
                {
                    Console.WriteLine($"{file}");

                    yield return ammo;

                    //only take 1 projectile from file
                    break;
                }
            }
        }
    }
}
