using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;
using Decima.HZD;
using AloysAdjustments.Models;
using AloysAdjustments.Utility;
using Newtonsoft.Json;
using Model = AloysAdjustments.Models.Model;

namespace AloysAdjustments
{
    public class Logic
    {
        public Logic() { }

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

            //var patch = Path.Combine(Configs.GamePackDir, IoC.Config.PatchFile);
            //using var rn = new FileRenamer(patch);

            //var file = await FileManager.ExtractFile(IoC.Decima,
            //    IoC.Config.TempPath, Configs.GamePackDir, false, @"entities/dlc1/characters/humanoids/uniquecharacters/dlc1_ikrie.core");

            //var objs = HzdCoreData.Load(file.Output);
            //var resources = objs.Select(CoreNode.FromObj).Where(x => x.Type.Name == "HumanoidBodyVariant");

            //models.AddRange(resources.Select(x =>
            //    new Model()
            //    {
            //        Name = x.Name,
            //        Id = x.Id
            //    }));

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
        
        public bool HasOutfitMap()
        {
            return File.Exists(IoC.Config.OutfitMapPath);
        }

        public async Task<OutfitFile[]> LoadOutfitMaps()
        {
            var json = await File.ReadAllTextAsync(IoC.Config.OutfitMapPath);

            return JsonConvert.DeserializeObject<OutfitFile[]>(json, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                Converters = new[] { new BaseGGUUIDConverter() }
            });
        }

        public async Task SaveOutfitMaps(OutfitFile[] maps)
        {
            var json = JsonConvert.SerializeObject(maps, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                Converters = new[] { new BaseGGUUIDConverter() }
            });

            await File.WriteAllTextAsync(IoC.Config.OutfitMapPath, json);
        }

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
        
        private async Task AddCharacterReferences(string path)
        {
            var components = await LoadPlayerComponents(path, true);
            var outfits = GetPlayerModels(components.Core);

            var models = await GenerateModelList();
            var id = models.First(x => x.Name == "DLC1_Ikrie");
            //var id = models[3];

            //var sRef = (StreamingRef<HumanoidBodyVariant>)outfits[33];

            //sRef.ExternalFile = new BaseString("entities/characters/humanoids/player/costumes/playercostume_norastealth_heavy");

            var armor = await FileManager.ExtractFile(IoC.Decima, path, Configs.GamePackDir, 
                true, "entities/characters/humanoids/player/costumes/playercostume_carjamatador_light.core");
            var ike = await FileManager.ExtractFile(IoC.Decima, IoC.Config.TempPath, Configs.GamePackDir,
                false, "entities/dlc1/characters/humanoids/uniquecharacters/dlc1_ikrie");
            
            var oArmor = HzdCore.Load(armor.Output);
            var oIke = HzdCore.Load(ike.Output);

            var mArmor = oArmor.GetTypes<ModelPartResource>().Values.First();
            var mIke = oIke.GetTypes<ModelPartResource>().Values.First();

            mArmor.MeshResource = mIke.MeshResource;
            mArmor.BoneBoundingBoxes = mIke.BoneBoundingBoxes;
            mArmor.PhysicsResource = mIke.PhysicsResource;
            mArmor.IsSkinned = mIke.IsSkinned;
            mArmor.PartMotionType = mIke.PartMotionType;
            mArmor.HelperNode = mIke.HelperNode;
            mArmor.Name = mIke.Name;

            oArmor.Save();
            
            //var sRef = new global::Decima.HZD.StreamingRef<HumanoidBodyVariant>();
            //sRef.ExternalFile = new BaseString("entities/dlc1/characters/humanoids/uniquecharacters/dlc1_ikrie");
            //sRef.Type = BaseRef<HumanoidBodyVariant>.Types.StreamingRef;
            //sRef.GUID = BaseGGUUID.FromOther(id.Id);

            //outfits.Add(sRef);

            //HzdCoreData.Save(components.File, components.Objects);
        }

        public bool CheckGameDir()
        {
            return Directory.Exists(Configs.GamePackDir);
        }
    }
}
