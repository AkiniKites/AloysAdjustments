using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Data;
using AloysAdjustments.Logic;
using AloysAdjustments.Utility;
using Decima.HZD;
using Model = AloysAdjustments.Data.Model;

namespace AloysAdjustments.Modules
{
    public class OutfitsLogic
    {
        public async Task<OutfitFile[]> GenerateOutfitFiles()
        {
            //TODO: Fix hack to ignore patch files
            var patch = Path.Combine(Configs.GamePackDir, IoC.Config.PatchFile);
            using var rn = new FileRenamer(patch);

            //extract game files
            var maps = await GenerateOutfitFilesFromPath(Configs.GamePackDir);
            return maps;
        }

        public async Task<OutfitFile[]> GenerateOutfitFilesFromPath(string path)
        {
            var files = await FileManager.ExtractFiles(IoC.Decima, 
                IoC.Config.TempPath, path, false, IoC.Config.OutfitFiles);

            var maps = files.Select(x => {
                var map = new OutfitFile { File = x.Source };

                var core = HzdCore.Load(x.Output);
                foreach (var item in GetOutfits(core))
                    map.Outfits.Add(item);

                return map;
            }).ToArray();
            //await SaveOutfitMaps(maps);

            await FileManager.Cleanup(IoC.Config.TempPath);

            return maps;
        }

        private IEnumerable<Outfit> GetOutfits(HzdCore core)
        {
            var items = core.GetTypes<InventoryEntityResource>().Values;
            var itemComponents = core.GetTypes<InventoryItemComponentResource>();
            var componentResources = core.GetTypes<NodeGraphComponentResource>();
            var overrides = core.GetTypes< OverrideGraphProgramResource>();
            var mappings = core.GetTypes< NodeGraphHumanoidBodyVariantUUIDRefVariableOverride>();

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
            //ignore duplicate names
            return files.SelectMany(x => x.Outfits)
                .GroupBy(x => x.ModelId).Select(x => x.First()).ToList();
        }

        public async Task<List<Model>> GenerateModelList()
        {
            var models = new List<Model>();

            //player models
            var playerComponents = (await LoadPlayerComponents(IoC.Config.TempPath)).Core;
            var playerModels = GetPlayerModels(playerComponents);

            models.AddRange(playerModels.Select(x => new Model
            {
                Id = x.GUID,
                Name = x.ExternalFile.ToString()
            }));

            return models;
        }

        public async Task<(HzdCore Core, string File)> LoadPlayerComponents(
            string outputPath, bool retainPath = false)
        {
            //TODO: Fix hack to ignore patch files
            var patch = Path.Combine(Configs.GamePackDir, IoC.Config.PatchFile);
            using var rn = new FileRenamer(patch);

            var file = await FileManager.ExtractFile(IoC.Decima,
                outputPath, Configs.GamePackDir, retainPath, IoC.Config.PlayerComponentsFile);

            var core = HzdCore.Load(file.Output);
            return (core, file.Output);
        }

        public List<global::Decima.HZD.StreamingRef<HumanoidBodyVariant>> GetPlayerModels(HzdCore core)
        {
            var resource = core.GetTypes<BodyVariantComponentResource>().FirstOrDefault().Value;
            if (resource == null)
                throw new HzdException("Unable to find PlayerBodyVariants");

            return resource.Variants;
        }

        public async Task CreatePatch(string patchDir, OutfitFile[] maps)
        {
            foreach (var map in maps)
            {
                //extract original outfit files to temp
                var file = await FileManager.ExtractFile(
                    IoC.Decima, patchDir, Configs.GamePackDir, true, map.File);

                var refs = map.Outfits.ToDictionary(x => x.RefId, x => x.ModelId);

                //update references from based on new maps
                var core = HzdCore.Load(file.Output);
                foreach (var reference in core.GetTypes<NodeGraphHumanoidBodyVariantUUIDRefVariableOverride>().Values)
                {
                    if (refs.TryGetValue(reference.ObjectUUID, out var newModel))
                        reference.Object.GUID.AssignFromOther(newModel);
                }

                core.Save();
            }
        }
    }
}
