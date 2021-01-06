using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private const string VariantNameFormat = "-AA-";
        private readonly Regex VariantNameMatcher = new Regex($"^(?<name>.+?){VariantNameFormat}(?<id>.+)$");

        public CharacterModelSearch Search { get; }

        public CharacterLogic()
        {
            Search = new CharacterModelSearch();
        }
        
        public async Task CreatePatch(string patchDir, IEnumerable<OutfitFile> maps, 
            IEnumerable<CharacterModel> characters, OutfitsLogic outfitsLogic)
        {
            var models = maps.SelectMany(x => x.Outfits).Select(x => x.ModelId).ToHashSet();
            var newCharacters = characters.Where(x => models.Contains(x.Id)).ToList();

            if (!newCharacters.Any())
                return;
            
            await RemoveAloyComponents(patchDir);
            var variantMapping = await FixRagdolls(patchDir, newCharacters);

            await AddCharacterReferences(patchDir, newCharacters, variantMapping);

            await outfitsLogic.CreatePatch(patchDir, maps, map =>
            {
                var mapping = new Dictionary<BaseGGUUID, BaseGGUUID>();
                foreach (var outfit in map.Outfits)
                {
                    if (variantMapping.TryGetValue(outfit.ModelId, out var varId))
                        mapping.Add(outfit.RefId, varId);
                    else
                        mapping.Add(outfit.RefId, outfit.ModelId);
                }
                return mapping;
            });
        }

        private async Task AddCharacterReferences(string patchDir, IEnumerable<CharacterModel> characters,
            Dictionary<BaseGGUUID, BaseGGUUID> variantMapping)
        {
            var pcCore = await FileManager.ExtractFile(patchDir, 
                Configs.GamePackDir, IoC.Get<OutfitConfig>().PlayerComponentsFile);
            var variants = OutfitsLogic.GetPlayerModels(pcCore);

            foreach (var character in characters)
            {
                var sRef = new StreamingRef<HumanoidBodyVariant>();
                sRef.ExternalFile = new BaseString(character.Source);
                sRef.Type = BaseRef<HumanoidBodyVariant>.Types.StreamingRef;

                if (!variantMapping.TryGetValue(character.Id, out var varId))
                    varId = character.Id;

                sRef.GUID = BaseGGUUID.FromOther(varId);

                variants.Add(sRef);
            }

            await pcCore.Save();
        }

        private async Task RemoveAloyComponents(string patchDir)
        {
            var charCore = await FileManager.ExtractFile(patchDir,
                Configs.GamePackDir, IoC.Get<OutfitConfig>().PlayerCharacterFile);

            var hairModel = charCore.GetTypes<HairModelComponentResource>().FirstOrDefault();
            if (hairModel == null)
                throw new HzdException($"Failed to remove Aloy's hair, unable to find HairModelComponentResource");

            var adult = charCore.GetTypes<SoldierResource>().FirstOrDefault(x=>x.Name == IoC.Get<OutfitConfig>().AloyCharacterName);
            if (adult == null)
                throw new HzdException($"Failed to remove Aloy's hair, unable to find SoldierResource with name: {IoC.Get<OutfitConfig>().AloyCharacterName}");

            var compCore = await IoC.Archiver.LoadFileAsync(Configs.GamePackDir, IoC.Get<OutfitConfig>().PlayerComponentsFile);

            var facePaint = compCore.GetTypes<FacialPaintComponentResource>().FirstOrDefault();
            if (facePaint == null)
                throw new HzdException($"Failed to remove Aloy's facepaint overrides, unable to find FacialPaintComponentResource");
            
            var toRemove = new[] { 
                hairModel.ObjectUUID,
                facePaint.ObjectUUID
            }.ToHashSet();

            adult.EntityComponentResources.RemoveAll(x => toRemove.Contains(x.GUID));

            await charCore.Save();
        }

        public async Task<bool> IsCharacterModeFile(string path)
        {
            //check if aloy has hair, if so the file was made for character edits
            var core = await IoC.Archiver.LoadFileAsync(path, 
                IoC.Get<OutfitConfig>().PlayerCharacterFile, false);

            if (core == null)
                return false;
            
            var hairModel = core.GetTypes<HairModelComponentResource>().FirstOrDefault();
            var adult = core.GetTypes<SoldierResource>().FirstOrDefault(x => x.Name == IoC.Get<OutfitConfig>().AloyCharacterName);

            if (hairModel == null || adult == null)
                return false;

            var hasHair = adult.EntityComponentResources.All(x => x.GUID.Equals(hairModel.ObjectUUID));

            return !hasHair;
        }

        private async Task<Dictionary<BaseGGUUID, BaseGGUUID>> FixRagdolls(
            string patchDir, IEnumerable<CharacterModel> characters)
        {
            var ragdollFile = IoC.Get<OutfitConfig>().RagdollComponentsFile;
            var variantMapping = new Dictionary<BaseGGUUID, BaseGGUUID>();

            foreach (var group in characters.GroupBy(x => x.Source))
            {
                var core = await FileManager.ExtractFile(patchDir,
                    Configs.GamePackDir, group.Key);

                var variants = core.GetTypesById<HumanoidBodyVariant>();
                foreach (var character in group)
                {
                    var variant = variants[character.Id];

                    if (variant.EntityComponentResources.Any(x => x.ExternalFile?.Value == ragdollFile))
                    {
                        //copy the variant
                        var newVariant = CopyVariant(variant);
                        variantMapping.Add(variant.ObjectUUID, newVariant.ObjectUUID);

                        //remove npc ragdoll repositioning
                        newVariant.EntityComponentResources.RemoveAll(x => x.ExternalFile?.Value == ragdollFile);

                        core.Components.Add(newVariant);
                    }
                }

                await core.Save();
            }

            return variantMapping;
        }

        private HumanoidBodyVariant CopyVariant(HumanoidBodyVariant variant)
        {
            var newVariant = HzdCloner.Clone(variant);

            newVariant.ObjectUUID = new GGUUID(BaseGGUUID.FromString($"{Guid.NewGuid():B}"));
            newVariant.Name = $"{variant.Name}{VariantNameFormat}{variant.ObjectUUID}";

            return newVariant;
        }

        public async Task<Dictionary<BaseGGUUID, BaseGGUUID>> GetVariantMapping(string path, OutfitsLogic outfitsLogic)
        {
            var variantMapping = new Dictionary<BaseGGUUID, BaseGGUUID>();

            var models = await outfitsLogic.GenerateModelList(path);
            foreach (var model in models)
            {
                var core = await IoC.Archiver.LoadFileAsync(path, model.Source, false);
                if (core == null) continue;

                if (!core.GetTypesById<HumanoidBodyVariant>().TryGetValue(model.Id, out var variant))
                    continue;

                var name = VariantNameMatcher.Match(variant.Name);
                if (name.Success)
                {
                    variantMapping.Add(model.Id, BaseGGUUID.FromString(name.Groups["id"].Value));
                }
            }

            return variantMapping;
        }
    }
}
