using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class OutfitsLogic
    {
        public async Task<OutfitFile[]> GenerateOutfitFiles()
        {
            //extract game files
            var maps = await GenerateOutfitFiles(Configs.GamePackDir, true);
            return maps;
        }

        public async Task<OutfitFile[]> GenerateOutfitFiles(string path, bool checkMissing)
        {
            var cores = await Task.WhenAll(IoC.Get<OutfitConfig>().OutfitFiles.Select(
                async f => await IoC.Archiver.LoadFileAsync(path, f, checkMissing)));

            var maps = cores.Where(x => x != null).Select(core =>
            {
                var map = new OutfitFile { File = core.Source };

                foreach (var item in GetOutfits(core))
                    map.Outfits.Add(item);

                return map;
            }).ToArray();

            return maps;
        }

        private IEnumerable<Outfit> GetOutfits(HzdCore core)
        {
            var items = core.GetTypes<InventoryEntityResource>();
            var itemComponents = core.GetTypesById<InventoryItemComponentResource>();
            var componentResources = core.GetTypesById<NodeGraphComponentResource>();
            var overrides = core.GetTypesById<OverrideGraphProgramResource>();
            var mappings = core.GetTypesById<NodeGraphHumanoidBodyVariantUUIDRefVariableOverride>();

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
                        outfit.LocalNameId = invItem.LocalizedItemName.GUID;
                        outfit.LocalNameFile = invItem.LocalizedItemName.ExternalFile?.ToString();
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
                                break;
                            }
                        }
                    }
                }

                yield return outfit;
            }
        }

        public List<Outfit> GenerateOutfitList(OutfitFile[] files)
        {
            return files.SelectMany(x => x.Outfits).ToList();
        }

        public async Task<List<Model>> GenerateModelList()
        {
            return await GenerateModelList(Configs.GamePackDir);
        }
        public async Task<List<Model>> GenerateModelList(string path)
        {
            var models = new List<Model>();

            //player models
            var playerComponents = await IoC.Archiver.LoadFileAsync(
                path, IoC.Get<OutfitConfig>().PlayerComponentsFile);
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

        public async Task CreatePatch(string patchDir, IEnumerable<OutfitFile> maps)
        {
            await CreatePatch(patchDir, maps,
                map => map.Outfits.ToDictionary(x => x.RefId, x => x.ModelId));
        }
        public async Task CreatePatch(string patchDir, IEnumerable<OutfitFile> maps,
            Func<OutfitFile, Dictionary<BaseGGUUID, BaseGGUUID>> getRefMapping)
        {
            foreach (var map in maps)
            {
                //extract original outfit files to temp
                var core = await FileManager.ExtractFile(patchDir, 
                    Configs.GamePackDir, map.File);

                var refs = getRefMapping(map);

                //update references from based on new maps
                foreach (var reference in core.GetTypes<NodeGraphHumanoidBodyVariantUUIDRefVariableOverride>())
                {
                    if (refs.TryGetValue(reference.ObjectUUID, out var newModel))
                        reference.Object.GUID.AssignFromOther(newModel);
                }

                await core.Save();
            }
        }
    }
}
