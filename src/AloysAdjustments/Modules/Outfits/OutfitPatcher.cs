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
            var details = BuildDetails(defaultOutfits, outfits, models).ToList();
            var characters = details.Where(x => x.Modified && x.IsCharacter).ToList();

            if (characters.Any())
            {
                //only update each character model once
                var chars = characters.GroupBy(x => x.Model.Id).Select(x => x.First());
                UpdateModelVariants(patch, chars, (core, outfit, variant) =>
                {
                    if (RemoveRagdollAI(variant))
                    {
                        //update all variants that use this model
                        foreach (var o in details.Where(x => x.VariantId == outfit.VariantId).ToList())
                        {
                            o.VariantId = variant.ObjectUUID;
                        }

                        return true;
                    }
                    return false;
                });
            }
            
            UpdateModelVariants(patch, details, (core, outfit, variant) => 
                AddOutfitFact(outfit.DefaultModel, core, variant));

            AddVariantReferences(patch, details);

            if (characters.Any())
            {
                UpdateComponents(patch, details);
            }

            UpdateOutfitRefs(patch, details);
        }

        public IEnumerable<OutfitDetail> BuildDetails(IList<Outfit> defaultOutfits, IList<Outfit> outfits, List<Model> models)
        {
            var modelSet = models.ToDictionary(x => x.Id, x => x);
            var defaultSet = defaultOutfits.ToDictionary(x => x.RefId, x=>x.ModelId);

            foreach (var outfit in outfits)
            {
                var detail = new OutfitDetail()
                {
                    Outfit = outfit,
                    Model = modelSet[outfit.ModelId],
                    DefaultModel = modelSet[defaultSet[outfit.RefId]],
                    VariantId = outfit.ModelId
                };

                yield return detail;
            }
        }

        private void AddVariantReferences(Patch patch, List<OutfitDetail> outfits)
        {
            //Add the new variant references to player components
            var pcCore = patch.AddFile(IoC.Get<OutfitConfig>().PlayerComponentsFile);
            var variants = OutfitsGenerator.GetPlayerModels(pcCore);

            var refs = variants.Select(x => x.GUID).ToHashSet();

            foreach (var outfit in outfits.Where(x => x.Modified))
            {
                if (!refs.Add(outfit.VariantId))
                    continue;

                var sRef = new StreamingRef<HumanoidBodyVariant>
                {
                    ExternalFile = new BaseString(outfit.Model.Source),
                    Type = BaseRef.Types.StreamingRef,
                    GUID = BaseGGUUID.FromOther(outfit.VariantId)
                };

                variants.Add(sRef);

                pcCore.Save();
            }
        }

        private void UpdateComponents(Patch patch, List<OutfitDetail> outfits)
        {
            //remove aloy components from player file
            var components = FindAloyComponents();
            RemoveAloyComponents(patch, components);

            //re-attach removed components to unchanged outfits and armor swaps 
            var reattachModels = outfits.Where(x => !x.Modified || !x.IsCharacter).Select(x => x.Model.Source);

            AttachAloyComponents(patch, reattachModels, components);
        }

        private List<(string File, BaseGGUUID Id)> FindAloyComponents()
        {
            //find components that conflict with character swaps
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

        private void UpdateModelVariants(Patch patch, IEnumerable<OutfitDetail> outfits, 
            Func<HzdCore, OutfitDetail, HumanoidBodyVariant, bool> updateVariant)
        {
            foreach (var group in outfits.Where(x => x.Modified).GroupBy(x => x.Model.Source))
            {
                var core = patch.AddFile(group.Key);

                var variants = core.GetTypesById<HumanoidBodyVariant>();
                foreach (var outfit in group)
                {
                    var variant = variants[outfit.VariantId];

                    //copy the variant
                    var newVariant = CopyVariant(variant);

                    if (updateVariant(core, outfit, newVariant))
                    {
                        outfit.VariantId = newVariant.ObjectUUID;
                        core.Binary.AddObject(newVariant);
                        core.Save();
                    }
                }
            }
        }

        private HumanoidBodyVariant CopyVariant(HumanoidBodyVariant variant)
        {
            var newVariant = HzdCloner.Clone(variant);

            newVariant.ObjectUUID = IoC.Uuid.New();

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
            //copy the fact from the default ourfit variant to the new variant
            //most are used for dialog, and carja disguise.
            //all character variants need an outfit fact otherwise the game will use the wrong one when checking
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
                    ObjectUUID = IoC.Uuid.New(),
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

        private void UpdateOutfitRefs(Patch patch, List<OutfitDetail> outfits)
        {
            var maps = outfits.Where(x => x.Modified).GroupBy(x => x.Outfit.SourceFile);

            foreach (var map in maps)
            {
                //extract original outfit files to temp
                var core = patch.AddFile(map.Key);

                //update references from based on new maps
                var refs = core.GetTypesById<NodeGraphHumanoidBodyVariantUUIDRefVariableOverride>();
                foreach (var outfit in map)
                {
                    if (refs.TryGetValue(outfit.Outfit.RefId, out var variantRef))
                        variantRef.Object.GUID.AssignFromOther(outfit.VariantId);
                }

                core.Save();
            }
        }
    }
}
