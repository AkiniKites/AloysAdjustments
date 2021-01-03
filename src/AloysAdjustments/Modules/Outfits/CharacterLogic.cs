using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Data;
using AloysAdjustments.Logic;
using AloysAdjustments.Utility;
using Decima.HZD;

namespace AloysAdjustments.Modules.Outfits
{
    public class CharacterLogic
    {
        private static readonly Regex HumanoidMatcher = new Regex("^entities/.*humanoids/.+");
        private static readonly Regex UniqueHumanoidMatcher = new Regex("^entities/.*humanoids/.*(?:individual_characters|uniquecharacters).+");
        private static readonly string[] Ignored = new string[]
        {
            "playercostume",
            "playercharacter",
            "debug"
        };

        public async Task<List<CharacterModel>> GetCharacterModels(bool all = false)
        {
            var files = (await Prefetch.LoadPrefetch()).Keys;

            var modelGroups = await Task.WhenAll(files.Select(async file => {
                if (!IsValid(file, all))
                    return null;
                return await GetCharacterModels(file);
            }));

            var models = modelGroups.Where(x => x != null).SelectMany(x => x).ToList();
            return models;
        }

        private bool IsValid(string file, bool all)
        {
            var matcher = all ? HumanoidMatcher : UniqueHumanoidMatcher;
            if (!matcher.IsMatch(file))
                return false;
            for (int i = 0; i < Ignored.Length; i++)
            {
                if (file.Contains(Ignored[i]))
                    return false;
            }

            return true;
        }

        private async Task<List<CharacterModel>> GetCharacterModels(string file)
        {
            var pack = await IoC.Archiver.LoadFile(Configs.GamePackDir, file);
            var variants = pack.GetTypes<HumanoidBodyVariant>().Values.ToList();
            if (!variants.Any())
                return null;

            Debug.WriteLine(file);

            var models = new List<CharacterModel>();
            foreach (var variant in variants)
            {
                models.Add(new CharacterModel()
                {
                    Id = variant.ObjectUUID,
                    Name = variant.Name,
                    Source = pack.Source
                }); ;
            }

            return models;
        }

        public async Task RemoveAloyHair(string patchDir)
        {
            var file = await FileManager.ExtractFile(patchDir,
                Configs.GamePackDir, true, IoC.Get<OutfitConfig>().PlayerCharacterFile);
            var core = HzdCore.Load(file, IoC.Get<OutfitConfig>().PlayerCharacterFile);

            var hairModel = core.GetTypes<HairModelComponentResource>().Values.FirstOrDefault();
            if (hairModel == null)
                throw new HzdException($"Failed to remove Aloy's hair, unable to find HairModelComponentResource");

            var adult = core.GetTypes<SoldierResource>().Values.FirstOrDefault(x=>x.Name == IoC.Get<OutfitConfig>().AloyCharacterName);
            if (adult == null)
                throw new HzdException($"Failed to remove Aloy's hair, unable to find SoldierResource with name: {IoC.Get<OutfitConfig>().AloyCharacterName}");
            
            adult.EntityComponentResources.RemoveAll(x => x.GUID.Equals(hairModel.ObjectUUID));
            core.Save();
        }
    }
}
