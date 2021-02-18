using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Plugins.Outfits.Data;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;
using Model = AloysAdjustments.Plugins.Common.Data.Model;

namespace AloysAdjustments.Plugins.Outfits
{
    public class OutfitsGenerator
    {
        public async Task<List<Outfit>> GenerateOutfits(Func<string, Task<HzdCore>> coreGetter)
        {
            var outfits = new List<Outfit>();

            var pcCore = await coreGetter(IoC.Get<OutfitConfig>().PlayerComponentsFile);

            var models = pcCore != null ? GetPlayerModels(pcCore) : new List<StreamingRef<HumanoidBodyVariant>>();
            var variantFiles = models.ToSoftDictionary(x => x.GUID, x => x.ExternalFile?.ToString());

            var files = IoC.Get<OutfitConfig>().OutfitFiles;
            var cores = await Task.WhenAll(files.Select(async f => await coreGetter(f)));

            foreach (var core in cores.Where(x => x != null))
            {
                foreach (var item in (await GetOutfits(core, variantFiles)))
                {
                    item.SourceFile = core.Source;
                    outfits.Add(item);
                }
            }

            return outfits;
        }

        private async Task<List<Outfit>> GetOutfits(HzdCore core, Dictionary<BaseGGUUID, string> variantFiles)
        {
            var items = core.GetTypes<InventoryEntityResource>();
            var itemComponents = core.GetTypesById<InventoryItemComponentResource>();
            var componentResources = core.GetTypesById<NodeGraphComponentResource>();
            var overrides = core.GetTypesById<OverrideGraphProgramResource>();
            var mappings = core.GetTypesById<NodeGraphHumanoidBodyVariantUUIDRefVariableOverride>();

            var outfits = new List<Outfit>();

            foreach (var item in items)
            {
                var outfit = new Outfit()
                {
                    Name = item.Name.ToString().Replace("InventoryEntityResource", "")
                };

                foreach (var component in item.EntityComponentResources)
                {
                    if (itemComponents.TryGetValue(component.GUID, out var invItem))
                    {
                        outfit.LocalName = await IoC.Localization.GetString(
                            invItem.LocalizedItemName.ExternalFile?.ToString(),
                            invItem.LocalizedItemName.GUID);
                    }

                    if (componentResources.TryGetValue(component.GUID, out var compRes))
                    {
                        var overrideRef = compRes.OverrideGraphProgramResource;
                        if (overrideRef?.GUID == null || !overrides.TryGetValue(overrideRef.GUID, out var rOverride))
                            continue;
                        
                        foreach (var mapRef in rOverride.VariableOverrides)
                        {
                            if (mappings.TryGetValue(mapRef.GUID, out var mapItem))
                            {
                                outfit.ModelId = mapItem.Object.GUID;
                                outfit.RefId = mapItem.ObjectUUID;

                                if (variantFiles.TryGetValue(outfit.ModelId, out var modelFile))
                                    outfit.ModelFile = modelFile;

                                break;
                            }
                        }
                    }
                }

                outfits.Add(outfit);
            }

            return outfits;
        }
        
        public List<Model> GenerateModelList(Func<string, HzdCore> coreGetter)
        {
            var models = new List<Model>();

            //player models
            var playerComponents = coreGetter(IoC.Get<OutfitConfig>().PlayerComponentsFile);
            var playerModels = GetPlayerModels(playerComponents);

            models.AddRange(playerModels.Select(x => new Model
            {
                Id = x.GUID,
                Name = GetModelName(x),
                Source = x.ExternalFile.ToString()
            }));

            return models;
        }
        private string GetModelName(StreamingRef<HumanoidBodyVariant> model)
        {
            var source = model.ExternalFile.ToString();

            var key = "playercostume_";
            var idx = source.LastIndexOf(key);
            return idx < 0 ? source : source.Substring(idx + key.Length);
        }

        public static List<StreamingRef<HumanoidBodyVariant>> GetPlayerModels(HzdCore core)
        {
            var resource = core.GetTypes<BodyVariantComponentResource>().FirstOrDefault();
            if (resource == null)
                throw new HzdException("Unable to find PlayerBodyVariants");

            return resource.Variants;
        }
    }
}
