using AloysAdjustments.Data;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model = AloysAdjustments.Data.Model;

namespace AloysAdjustments.Modules.Outfits
{
    public class OutfitPatcher
    {
        public void CreatePatch(Patch patch, IList<Outfit> defaultOutfits, IList<Outfit> outfits, List<Model> models)
        {
            var details = BuildDetails(defaultOutfits, outfits, models).ToList();
            var hasCharacters = details.Any(x => x.Modified && x.IsCharacter);

            if (hasCharacters)
            {
                //remove aloy components from player file
                var components = FindAloyComponents();
                RemoveAloyComponents(patch, components);

                UpdateModelVariants(patch, details, (outfit, core, variant) =>
                    RemoveRagdollAI(outfit, variant));
                UpdateModelVariants(patch, details, (outfit, core, variant) =>
                    AttachAloyComponents(outfit, variant, components));
            }
            
            UpdateModelVariants(patch, details, UpdateArmorFact);
            UpdateModelVariants(patch, details, (outfit, core, variant) =>
                UpdateArmorBuffs(outfit, variant));

            AddVariantReferences(patch, details);
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
            Func<OutfitDetail, HzdCore, HumanoidBodyVariant, bool> updateVariant)
        {
            foreach (var group in outfits.Where(x => x.Modified).GroupBy(x => x.Model.Source))
            {
                var core = patch.AddFile(group.Key);

                var changes = new List<(BaseGGUUID SourceId, BaseGGUUID NewId, 
                    HumanoidBodyVariant Data, Action collapse)>();
                var variants = core.GetTypesById<HumanoidBodyVariant>();

                foreach (var outfit in group)
                {
                    var variant = variants[outfit.VariantId];

                    //copy the variant
                    var newVariant = CopyVariant(variant, out var isOriginal);

                    if (updateVariant(outfit, core, newVariant))
                    {
                        void collapseVariant()
                        {
                            if (isOriginal) return;
                            
                            core.Binary.RemoveObject(variant);
                            newVariant.ObjectUUID = variant.ObjectUUID;
                            outfit.VariantId = variant.ObjectUUID;
                        }

                        var change = changes.FirstOrDefault(x => HzdUtils.EqualsIgnoreId(x.Data, newVariant));
                        if (change.NewId != null)
                        {
                            changes.Add((change.SourceId, change.NewId, null, () =>
                            {
                                if (isOriginal) return;

                                outfit.VariantId = variant.ObjectUUID;
                            }));

                            outfit.VariantId = change.NewId;
                            continue;
                        }
                        
                        changes.Add((variant.ObjectUUID, newVariant.ObjectUUID, newVariant, collapseVariant));
                        outfit.VariantId = newVariant.ObjectUUID;
                        core.Binary.AddObject(newVariant);
                    }
                }

                //only 1 new variant copy, collapse changes into original
                foreach (var changeGroup in changes.GroupBy(x => x.SourceId)
                    .Where(g => g.Count(x => x.Data != null) == 1))
                {
                    foreach (var change in changeGroup)
                        change.collapse();
                }

                if (changes.Any())
                {
                    core.Save();
                }
            }
        }

        private HumanoidBodyVariant CopyVariant(HumanoidBodyVariant variant, out bool isOriginal)
        {
            var newVariant = HzdUtils.Clone(variant);

            newVariant.ObjectUUID = IoC.Uuid.New();

            //rename if not a copy
            if (!CharacterGenerator.VariantNameMatcher.IsMatch(variant.Name))
            {
                newVariant.Name = $"{variant.Name}{CharacterGenerator.VariantNameFormat}{variant.ObjectUUID}";
                isOriginal = true;
            }
            else
            {
                isOriginal = false;
            }

            return newVariant;
        }

        private bool RemoveRagdollAI(OutfitDetail outfit, HumanoidBodyVariant variant)
        {
            if (!outfit.IsCharacter)
                return false;

            var ragdollFile = IoC.Get<OutfitConfig>().RagdollComponentsFile;

            //remove npc ragdoll repositioning
            var removed =  variant.EntityComponentResources.RemoveAll(x => x.ExternalFile == ragdollFile);
            return removed > 0;
        }

        private bool UpdateArmorFact(OutfitDetail outfit, HzdCore core, HumanoidBodyVariant variant)
        {
            var changed = false;
            var factValue = GetArmorFact(outfit.DefaultModel);

            //strip away the default variant fact for the armor we're swapping to
            if (!outfit.IsCharacter)
            {
                var defaultFact = GetArmorFact(outfit.Model);
                if (defaultFact != null)
                {
                    if (defaultFact.FactValueBase.Value.Value == factValue.FactValueBase.Value.Value)
                        return false;

                    changed = variant.Facts.RemoveAll(x => x.GUID == defaultFact.ObjectUUID) > 0;
                }
            }

            //copy the fact from the default outfit variant to the new variant
            //most are used for dialog, and carja disguise.
            //all character variants need an outfit fact otherwise the game will use the wrong one when checking
            var factFile = IoC.Get<OutfitConfig>().OutfitFactFile;
            
            //already has outfit fact
            var fact = core.GetTypes<FactValue>().FirstOrDefault(x => 
                x.FactValueBase.Value.Value == factValue.FactValueBase.Value.Value);
            if (fact != null && variant.Facts.Any(x => x.GUID == fact.ObjectUUID))
                return changed;
            
            if (fact == null)
            {
                fact = new FactValue()
                {
                    ObjectUUID = IoC.Uuid.New(),
                    FactValueBase = new FactValueBase()
                    {
                        Value = factValue.FactValueBase.Value.Value,
                        Fact = new Ref<Fact>()
                        {
                            GUID = factValue.FactValueBase.Fact.GUID,
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

        private FactValue GetArmorFact(Model outfitModel)
        {
            var factFile = IoC.Get<OutfitConfig>().OutfitFactFile;
            var modelCore = IoC.Archiver.LoadGameFile(outfitModel.Source);
            var factEnum = modelCore.GetTypes<FactValue>().FirstOrDefault(x => x.FactValueBase.Fact.ExternalFile == factFile);
            if (factEnum == null)
                return null;

            var variant = modelCore.GetTypes<HumanoidBodyVariant>().First(x => x.ObjectUUID == outfitModel.Id);
            if (variant.Facts.Any(x => x.GUID == factEnum.ObjectUUID))
                return factEnum;
            return null;
        }

        private bool UpdateArmorBuffs(OutfitDetail outfit, HumanoidBodyVariant variant)
        {
            var changed = false;

            //strip away the default variant components for the armor we're swapping to
            if (!outfit.IsCharacter)
            {
                var defaultBuffs = GetArmorBuffs(outfit.Model);
                changed = variant.EntityComponentResources
                    .RemoveAll(x => defaultBuffs.Any(b => b.GUID == x.GUID)) > 0;
            }

            //add the components from the new armor
            foreach (var buff in GetArmorBuffs(outfit.DefaultModel))
            {
                changed = true;
                variant.EntityComponentResources.Add(buff);
            }

            return changed;
        }

        private IEnumerable<Ref<EntityComponentResource>> GetArmorBuffs(Model outfitModel)
        {
            //check if default model has any regen components
            var componentFiles = IoC.Get<OutfitConfig>().ArmorComponentFiles.ToHashSet();
            var ignoredTypes = IoC.Get<OutfitConfig>().IgnoredArmorComponents.ToHashSet();

            var modelCore = IoC.Archiver.LoadGameFile(outfitModel.Source);
            var variant = modelCore.GetTypes<HumanoidBodyVariant>().First(x => x.ObjectUUID == outfitModel.Id);

            var componentSetRefs = variant.EntityComponentResources
                .Where(x => componentFiles.Contains(x.ExternalFile)).ToList();
            
            foreach (var setRef in componentSetRefs)
            {
                var components = IoC.Archiver.LoadGameFile(setRef.ExternalFile).GetTypesById();
                if (!(components[setRef.GUID] is EntityComponentSetResource setResource))
                    continue;
                //ignore the set with skeleton and scaling helpers
                if (setResource.ComponentResources.All(x => ignoredTypes.Contains(components[x.GUID].GetType().Name)))
                    continue;

                yield return setRef;
            }
        }

        private bool AttachAloyComponents(OutfitDetail outfit, HumanoidBodyVariant variant, 
            List<(string File, BaseGGUUID Id)> components)
        {
            //re-attach removed components to unchanged outfits and armor swaps 
            if (outfit.IsCharacter || outfit.Modified)
                return false;

            foreach (var item in components)
            {
                var comp = new Ref<EntityComponentResource>()
                {
                    GUID = item.Id,
                    ExternalFile = new BaseString(item.File),
                    Type = BaseRef.Types.ExternalCoreUUID
                };

                variant.EntityComponentResources.Add(comp);
            }

            return true;
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
