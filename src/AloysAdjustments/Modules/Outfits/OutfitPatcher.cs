using System;
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
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;
using Model = AloysAdjustments.Data.Model;

namespace AloysAdjustments.Modules.Outfits
{
    public class OutfitPatcher
    {
        public void CreatePatch(Patch patch, IList<Outfit> defaultOutfits, IList<Outfit> outfits, List<Model> models)
        {
            var modelSet = models.ToDictionary(x => x.Id, x => x);
            var outfitMap = outfits.Where(x => x.Modified).Select(x => (Outfit: x, Model: modelSet[x.ModelId])).ToList();
            var defaultOutfitMap = defaultOutfits.Select(x => (Outfit: x, Model: modelSet[x.ModelId])).ToList();
            var variantMapping = outfitMap.ToDictionary(x => x.Outfit.RefId, x => x.Model.Id);

            var activeModels = outfits.Where(x => x.Modified).Select(x => x.ModelId).ToHashSet();
            var newCharacters = models.Where(x => x is CharacterModel && activeModels.Contains(x.Id)).Cast<CharacterModel>().ToList();

            if (newCharacters.Any())
            {
                FixRagdolls(patch, newCharacters, variantMapping);
            }

            AddOutfitFacts(patch, defaultOutfitMap, outfitMap, variantMapping);

            //Add the new variant references to player components
            AddVariantReferences(patch, outfitMap, variantMapping);

            if (newCharacters.Any())
            {
                UpdateComponents(patch, outfits, models);
            }

            CreatePatch(patch, outfits, variantMapping);
        }

        private void AddVariantReferences(Patch patch, IEnumerable<(Outfit Outfit, Model Model)> outfitMaps,
            Dictionary<BaseGGUUID, BaseGGUUID> variantMapping)
        {
            var pcCore = patch.AddFile(IoC.Get<OutfitConfig>().PlayerComponentsFile);
            var variants = OutfitsGenerator.GetPlayerModels(pcCore);

            var refs = variants.Select(x=>x.GUID).ToHashSet();

            foreach (var character in outfitMaps)
            {
                if (!variantMapping.TryGetValue(character.Outfit.RefId, out var varId))
                    varId = character.Model.Id;

                if (!refs.Add(varId))
                    continue;

                var sRef = new StreamingRef<HumanoidBodyVariant>
                {
                    ExternalFile = new BaseString(character.Model.Source),
                    Type = BaseRef.Types.StreamingRef,
                    GUID = BaseGGUUID.FromOther(varId)
                };

                variants.Add(sRef);

                pcCore.Save();
            }
        }

        private void UpdateComponents(Patch patch, IList<Outfit> outfits, List<Model> models)
        {
            var activeModels = outfits.Where(x => x.Modified).Select(x => x.ModelId).ToHashSet();

            //remove aloy components from player file
            var removedComponents = FindAloyComponents();
            RemoveAloyComponents(patch, removedComponents);

            //attach removed components to non-character outfit changes
            var unchangedOutfits = outfits.Where(x => !x.Modified).Select(x => x.ModelFile).ToList();
            var remappedOutfits = models.Where(x => !(x is CharacterModel) && activeModels.Contains(x.Id)).Select(x => x.Source);
            AttachAloyComponents(patch, unchangedOutfits.Concat(remappedOutfits), removedComponents);
        }

        private List<(string File, BaseGGUUID Id)> FindAloyComponents()
        {
            var components = new  List<(string, BaseGGUUID)>();

            var charCore = IoC.Archiver.LoadGameFile(IoC.Get<OutfitConfig>().PlayerCharacterFile);

            var hairModel = charCore.GetType<HairModelComponentResource>();
            if (hairModel == null)
                throw new HzdException($"Failed to remove Aloy's hair, unable to find HairModelComponentResource");

            components.Add((charCore.Source, hairModel.ObjectUUID));
            
            var compCore = IoC.Archiver.LoadGameFile(IoC.Get<OutfitConfig>().PlayerComponentsFile);

            var facePaint = compCore.GetType<FacialPaintComponentResource>();
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

        private void FixRagdolls(Patch patch, IEnumerable<CharacterModel> characters, 
            Dictionary<BaseGGUUID, BaseGGUUID> variantMapping)
        {
            foreach (var group in characters.GroupBy(x => x.Source))
            {
                var core = patch.AddFile(group.Key);

                var variants = core.GetTypesById<HumanoidBodyVariant>();
                foreach (var character in group)
                {
                    var variant = variants[character.Id];

                    //copy the variant
                    var newVariant = CopyVariant(variant);

                    if (RemoveRagdollAI(newVariant))
                    {
                        //update all variants that use this model
                        foreach (var kv in variantMapping.Where(x => x.Value == variant.ObjectUUID).ToList())
                        {
                            variantMapping[kv.Key] = newVariant.ObjectUUID;
                        }

                        core.Binary.AddObject(newVariant);
                        core.Save();
                    }
                }
            }
        }

        private void AddOutfitFacts(Patch patch, 
            IList<(Outfit Outfit, Model Model)> defaultOutfitMap,
            IList<(Outfit Outfit, Model Model)> outfitMaps, 
            Dictionary<BaseGGUUID, BaseGGUUID> variantMapping)
        {
            foreach (var group in outfitMaps.GroupBy(x => x.Model.Source))
            {
                var core = patch.AddFile(group.Key);

                var variants = core.GetTypesById<HumanoidBodyVariant>();
                foreach (var outfitMap in group)
                {
                    var variant = variants[variantMapping[outfitMap.Outfit.RefId]];
                    var defaultModel = defaultOutfitMap.First(x => outfitMap.Outfit.RefId == x.Outfit.RefId).Model;

                    //copy the variant
                    var newVariant = CopyVariant(variant);

                    //every model needs an outfit fact otherwise the game will just use the last one equipped
                    if (AddOutfitFact(defaultModel, core, newVariant))
                    {
                        variantMapping[outfitMap.Outfit.RefId] = newVariant.ObjectUUID;
                        core.Binary.AddObject(newVariant);
                        core.Save();
                    }
                }
            }
        }

        private HumanoidBodyVariant CopyVariant(HumanoidBodyVariant variant)
        {
            var newVariant = HzdCloner.Clone(variant);

            newVariant.ObjectUUID = Guid.NewGuid();

            //rename if not a copy
            if (!CharacterGenerator.VariantNameMatcher.IsMatch(variant.Name))
                newVariant.Name = $"{variant.Name}{CharacterGenerator.VariantNameFormat}{variant.ObjectUUID}";

            return newVariant;
        }

        private bool RemoveRagdollAI(HumanoidBodyVariant variant)
        {
            var ragdollFile = IoC.Get<OutfitConfig>().RagdollComponentsFile;

            //remove npc ragdoll repositioning
            var removed =  variant.EntityComponentResources.RemoveAll(x => x.ExternalFile == ragdollFile);
            return removed > 0;
        }

        private bool AddOutfitFact(Model defaultModel, HzdCore core, HumanoidBodyVariant variant)
        {
            var factFile = IoC.Get<OutfitConfig>().OutfitFactFile;
            var modelCore = IoC.Archiver.LoadGameFile(defaultModel.Source);
            var factEnum = modelCore.GetTypes<FactValue>().FirstOrDefault(x => x.FactValueBase.Fact.ExternalFile == factFile);
            
            //already has outfit fact
            var fact = core.GetTypes<FactValue>().FirstOrDefault(x => 
                x.FactValueBase.Value.Value == factEnum.FactValueBase.Value.Value);
            if (fact != null && variant.Facts.Any(x => x.GUID == fact.ObjectUUID))
                return false;
            
            if (fact == null)
            {
                fact = new FactValue()
                {
                    ObjectUUID = Guid.NewGuid(),
                    FactValueBase = new FactValueBase()
                    {
                        Value = factEnum.FactValueBase.Value.Value,
                        Fact = new Ref<Fact>()
                        {
                            GUID = factEnum.FactValueBase.Fact.GUID,
                            ExternalFile = factFile,
                            Type = BaseRef.Types.ExternalCoreUUID
                        }
                    }
                };

                core.Binary.AddObject(fact);
            }

            variant.Facts.Add(new Ref<FactValue>()
            {
                GUID = fact.ObjectUUID,
                Type = BaseRef.Types.LocalCoreUUID
            });

            return true;
        }

        private void AttachAloyComponents(Patch patch, IEnumerable<string> outfitFiles, 
            List<(string File, BaseGGUUID Id)> removed)
        {
            foreach (var file in outfitFiles.Distinct())
            {
                var core = patch.AddFile(file);

                var variants = core.GetTypes<HumanoidBodyVariant>();
                foreach (var variant in variants)
                {
                    foreach (var item in removed)
                    {
                        var comp = new Ref<EntityComponentResource>()
                        {
                            GUID = item.Id,
                            ExternalFile = new BaseString(item.File),
                            Type = BaseRef.Types.ExternalCoreUUID
                        };

                        variant.EntityComponentResources.Add(comp);
                        core.Save();
                    }
                }
            }
        }

        private void CreatePatch(Patch patch, IList<Outfit> outfits,
            Dictionary<BaseGGUUID, BaseGGUUID> variantMapping)
        {
            var maps = outfits.Where(x => x.Modified).Select(x => x.SourceFile).Distinct();

            foreach (var map in maps)
            {
                //extract original outfit files to temp
                var core = patch.AddFile(map);

                //update references from based on new maps
                foreach (var reference in core.GetTypes<NodeGraphHumanoidBodyVariantUUIDRefVariableOverride>())
                {
                    if (variantMapping.TryGetValue(reference.ObjectUUID, out var variantId))
                        reference.Object.GUID.AssignFromOther(variantId);
                }

                core.Save();
            }
        }
    }
}
