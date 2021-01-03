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
using Decima;
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

        public async Task CreatePatch(string patchDir, IEnumerable<CharacterModel> characters, IEnumerable<OutfitFile> maps)
        {
            var models = maps.SelectMany(x => x.Outfits).Select(x => x.ModelId).ToHashSet();
            var newCharacters = characters.Where(x => models.Contains(x.Id));

            await AddCharacterReferences(patchDir, newCharacters);
            await RemoveAloyHair(patchDir);
        }

        private async Task AddCharacterReferences(string patchDir, IEnumerable<CharacterModel> characters)
        {
            var pcCore = await FileManager.ExtractFile(patchDir, 
                Configs.GamePackDir, IoC.Get<OutfitConfig>().PlayerComponentsFile);
            var variants = OutfitsLogic.GetPlayerModels(pcCore);

            foreach (var character in characters)
            {
                var sRef = new StreamingRef<HumanoidBodyVariant>();
                sRef.ExternalFile = new BaseString(character.Source);
                sRef.Type = BaseRef<HumanoidBodyVariant>.Types.StreamingRef;
                sRef.GUID = BaseGGUUID.FromOther(character.Id);

                variants.Add(sRef);
            }

            pcCore.Save();
        }

        public async Task RemoveAloyHair(string patchDir)
        {
            var core = await FileManager.ExtractFile(patchDir,
                Configs.GamePackDir, IoC.Get<OutfitConfig>().PlayerCharacterFile);

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
