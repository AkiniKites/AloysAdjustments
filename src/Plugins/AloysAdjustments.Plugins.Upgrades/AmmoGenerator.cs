﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Plugins.Upgrades.Data;
using AloysAdjustments.Utility;
using Decima.HZD;

namespace AloysAdjustments.Plugins.Upgrades
{
    public class AmmoGenerator
    {
        private readonly Regex AmmoMatcher;
        private readonly string[] Ignored;

        private readonly FileCollector<AmmoItem> _ammoFileCollector;

        public AmmoGenerator()
        {
            //HumanoidMatcher = new Regex(IoC.Get<UpgradeConfig>().HumanoidMatcher);
            AmmoMatcher = new Regex("^entities/.*weapons/ammo/.+");
            //Ignored = IoC.Get<UpgradeConfig>().IgnoredCharacters.ToArray();
            Ignored = new string[0];

            _ammoFileCollector = new FileCollector<AmmoItem>("ammo",
                IsAmmoFile, GetAmmoItems, Ignored);
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
            var stackables = pack.GetTypes<UpgradableStackableComponentResource>();
            
            foreach (var stackable in stackables)
            {
                yield return new AmmoItem()
                {
                    Id = stackable.ObjectUUID,
                    File = file,
                    FactId = stackable.UpgradeLevelFact.GUID,
                    FactFile = stackable.UpgradeLevelFact.ExternalFile
                };

                //var fact = IoC.Archiver.LoadGameFile(stackables[0].UpgradeLevelFact.ExternalFile);
                //var intFact = fact.GetTypesById<IntegerFact>()[stackables[0].UpgradeLevelFact.GUID];



                //Debug.WriteLine(file + " :: " + intFact.Name + " :: " + String.Join(",", stackables[0].UpgradedLimits));
                
            }
        }
    }
}