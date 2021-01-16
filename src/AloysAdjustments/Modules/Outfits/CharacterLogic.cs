﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Model = AloysAdjustments.Data.Model;

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
        
        public Dictionary<BaseGGUUID, BaseGGUUID> CreatePatch(
            Patch patch, ReadOnlyCollection<Outfit> outfits, List<Model> models)
        {
            var activeModels = outfits.Where(x => x.Modified).Select(x => x.ModelId).ToHashSet();
            var newCharacters = models.Where(x => x is CharacterModel && activeModels.Contains(x.Id)).Cast<CharacterModel>().ToList();

            if (!newCharacters.Any())
                return new Dictionary<BaseGGUUID, BaseGGUUID>();

            //remove aloy components from player file
            var toRemove = FindAloyComponents();
            RemoveAloyComponents(patch, toRemove);

            //fix character variants
            var variantMapping = FixRagdolls(patch, newCharacters);

            //update outfit mappings
            AddCharacterReferences(patch, newCharacters, variantMapping);

            //attach removed components to non-character outfit changes
            var unchanged = outfits.Where(x => !x.Modified).Select(x => x.ModelFile).ToList();
            var newOutfits = models.Where(x => !(x is CharacterModel) && activeModels.Contains(x.Id)).Select(x => x.Source);
            AttachAloyComponents(patch, unchanged.Concat(newOutfits), toRemove);

            return variantMapping;
        }

        private void AddCharacterReferences(Patch patch, IEnumerable<CharacterModel> characters,
            Dictionary<BaseGGUUID, BaseGGUUID> variantMapping)
        {
            var pcCore = patch.AddFile(IoC.Get<OutfitConfig>().PlayerComponentsFile);
            var variants = OutfitsLogic.GetPlayerModels(pcCore);

            foreach (var character in characters)
            {
                var sRef = new StreamingRef<HumanoidBodyVariant>();
                sRef.ExternalFile = new BaseString(character.Source);
                sRef.Type = BaseRef.Types.StreamingRef;

                if (!variantMapping.TryGetValue(character.Id, out var varId))
                    varId = character.Id;

                sRef.GUID = BaseGGUUID.FromOther(varId);

                variants.Add(sRef);
            }

            pcCore.Save();
        }
        
        private List<(string File, BaseGGUUID Id)> FindAloyComponents()
        {
            var components = new  List<(string, BaseGGUUID)>();

            var charCore = IoC.Archiver.LoadFile(Configs.GamePackDir, 
                IoC.Get<OutfitConfig>().PlayerCharacterFile);

            var hairModel = charCore.GetTypes<HairModelComponentResource>().FirstOrDefault();
            if (hairModel == null)
                throw new HzdException($"Failed to remove Aloy's hair, unable to find HairModelComponentResource");

            components.Add((charCore.Source, hairModel.ObjectUUID));
            
            var compCore = IoC.Archiver.LoadFile(Configs.GamePackDir, IoC.Get<OutfitConfig>().PlayerComponentsFile);

            var facePaint = compCore.GetTypes<FacialPaintComponentResource>().FirstOrDefault();
            if (facePaint == null)
                throw new HzdException($"Failed to remove Aloy's facepaint overrides, unable to find FacialPaintComponentResource");

            components.Add((compCore.Source, facePaint.ObjectUUID));
            
            return components;
        }

        private void RemoveAloyComponents(Patch patch, List<(string File, BaseGGUUID Id)> components)
        {
            var core = patch.AddFile(IoC.Get<OutfitConfig>().PlayerCharacterFile);
            
            var adult = core.GetTypes<SoldierResource>().FirstOrDefault(x=>x.Name == IoC.Get<OutfitConfig>().AloyCharacterName);
            if (adult == null)
                throw new HzdException($"Failed to remove Aloy's hair, unable to find SoldierResource with name: {IoC.Get<OutfitConfig>().AloyCharacterName}");

            var toRemove = components.Select(x => x.Id).ToHashSet();

            adult.EntityComponentResources.RemoveAll(x => toRemove.Contains(x.GUID));

            core.Save();
        }

        private Dictionary<BaseGGUUID, BaseGGUUID> FixRagdolls(
            Patch patch, IEnumerable<CharacterModel> characters)
        {
            var cCore = IoC.Archiver.LoadFile(Configs.GamePackDir,
                $"entities/characters/humanoids/player/costumes/playercostume_carjasoldier_light.core");
            var fact = cCore.GetTypes<FactValue>().Last();

            var ragdollFile = IoC.Get<OutfitConfig>().RagdollComponentsFile;
            var variantMapping = new Dictionary<BaseGGUUID, BaseGGUUID>();

            foreach (var group in characters.GroupBy(x => x.Source))
            {
                var core = patch.AddFile(group.Key);

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
                        newVariant.Facts.Add(new Ref<FactValue>()
                        {
                            GUID = fact.ObjectUUID,
                            Type =BaseRef.Types.LocalCoreUUID 
                        });

                        core.Binary.AddObject(fact);
                        core.Binary.AddObject(newVariant);
                    }
                }

                core.Save();
            }

            return variantMapping;
        }

        private HumanoidBodyVariant CopyVariant(HumanoidBodyVariant variant)
        {
            var newVariant = HzdCloner.Clone(variant);

            newVariant.ObjectUUID = $"{Guid.NewGuid():B}";
            newVariant.Name = $"{variant.Name}{VariantNameFormat}{variant.ObjectUUID}";

            return newVariant;
        }

        private void AttachAloyComponents(Patch patch, IEnumerable<string> outfitFiles, List<(string File, BaseGGUUID Id)> removed)
        {
            foreach (var file in outfitFiles.Distinct())
            {
                var core = patch.AddFile(file);

                var resources = core.GetTypes<HumanoidBodyVariant>().First().EntityComponentResources;
                foreach (var item in removed)
                {
                    var comp = new Ref<EntityComponentResource>()
                    {
                        GUID = item.Id,
                        ExternalFile = new BaseString(item.File),
                        Type = BaseRef.Types.ExternalCoreUUID
                    };

                    resources.Add(comp);
                }

                core.Save();
            }
        }

        public Dictionary<BaseGGUUID, BaseGGUUID> GetVariantMapping(string path, OutfitsLogic outfitsLogic)
        {
            var variantMapping = new Dictionary<BaseGGUUID, BaseGGUUID>();

            var models = outfitsLogic.GenerateModelList(path);
            foreach (var model in models)
            {
                var core = IoC.Archiver.LoadFile(path, model.Source, false);
                if (core == null) continue;

                if (!core.GetTypesById<HumanoidBodyVariant>().TryGetValue(model.Id, out var variant))
                    continue;

                var name = VariantNameMatcher.Match(variant.Name);
                if (name.Success)
                {
                    variantMapping.Add(model.Id, name.Groups["id"].Value);
                }
            }

            return variantMapping;
        }
    }
}
