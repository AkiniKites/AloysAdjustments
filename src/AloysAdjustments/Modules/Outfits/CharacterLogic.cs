using System;
using System.Collections.Concurrent;
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
        public CharacterModelSearch Search { get; }

        public CharacterLogic()
        {
            Search = new CharacterModelSearch();
        }

        public async Task CreatePatch(string patchDir, IEnumerable<CharacterModel> characters, IEnumerable<OutfitFile> maps)
        {
            var models = maps.SelectMany(x => x.Outfits).Select(x => x.ModelId).ToHashSet();
            var newCharacters = characters.Where(x => models.Contains(x.Id)).ToList();

            if (newCharacters.Any())
            {
                await AddCharacterReferences(patchDir, newCharacters);
                await RemoveAloyHair(patchDir);
                await FixRagdolls(patchDir, newCharacters);
            }
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

        private async Task RemoveAloyHair(string patchDir)
        {
            var core = await FileManager.ExtractFile(patchDir,
                Configs.GamePackDir, IoC.Get<OutfitConfig>().PlayerCharacterFile);

            var hairModel = core.GetTypes<HairModelComponentResource>().FirstOrDefault();
            if (hairModel == null)
                throw new HzdException($"Failed to remove Aloy's hair, unable to find HairModelComponentResource");

            var adult = core.GetTypes<SoldierResource>().FirstOrDefault(x=>x.Name == IoC.Get<OutfitConfig>().AloyCharacterName);
            if (adult == null)
                throw new HzdException($"Failed to remove Aloy's hair, unable to find SoldierResource with name: {IoC.Get<OutfitConfig>().AloyCharacterName}");
            
            adult.EntityComponentResources.RemoveAll(x => x.GUID.Equals(hairModel.ObjectUUID));
            core.Save();
        }

        private async Task FixRagdolls(string patchDir, IEnumerable<CharacterModel> characters)
        {
            foreach (var character in characters)
            {
                var core = await FileManager.ExtractFile(patchDir,
                    Configs.GamePackDir, character.Source);

                var hbv = core.GetTypes<HumanoidBodyVariant>().FirstOrDefault();

                //core.GetTypes<ScaleEntityFromAnimationComponentResource>().FirstOrDefault().ScaleMessageName = "ScaleTo";
                hbv.EntityComponentResources.RemoveAt(2);
                //hbv.EntityComponentResources.Add(comp);

                core.Save();
            }
        }
    }
}
