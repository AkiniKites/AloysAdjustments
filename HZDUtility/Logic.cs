using Decima;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima.HZD;
using HZDUtility.Models;
using HZDUtility.Utility;
using Model = HZDUtility.Models.Model;

namespace HZDUtility
{
    public class Logic
    {
        private const string PatchTempDir = "patch";
        
        public Logic() { }

        public List<Outfit> GenerateOutfitList(OutfitFile[] files)
        {
            //ignore duplicate names
            return files.SelectMany(x => x.Outfits)
                .GroupBy(x => x.Id).Select(x => x.First()).ToList();
        }

        public async Task<(List<object> Objects, string File)> LoadPlayerComponents(
            string path, bool retainPath = false)
        {
            //TODO: Fix hack to ignore patch files
            var patch = Path.Combine(Configs.GamePackDir, IoC.Config.PatchFile);
            using var rn = new FileRenamer(patch);

            var file = await FileManager.ExtractFile(IoC.Decima,
                path, Configs.GamePackDir, retainPath, IoC.Config.PlayerComponentsFile);

            if (file.Output == null)
                throw new HzdException($"Unable to find {IoC.Config.PlayerComponentsFile}");

            var objs = CoreBinary.Load(file.Output);
            return (objs, file.Output);
        }

        public IEnumerable<Outfit> GetOutfitList(List<object> components)
        {
            var outfits = GetVariants(components);
            return outfits.Cast<object>().Select(CoreNode.FromObj).Select(x =>
                new Outfit()
                {
                    Name = x.GetString("ExternalFile"),
                    Id = x.GetField<BaseGGUUID>("GUID")
                });
        }

        public IList GetVariants(List<object> components)
        {
            //BodyVariantComponentResource
            var resource = components.Select(CoreNode.FromObj).FirstOrDefault(x => x.Name == "PlayerBodyVariants");
            if (resource == null)
                throw new HzdException("Unable to find PlayerBodyVariants");

            return resource.GetField<IList>("Variants");
        }

        public async Task<List<Model>> GenerateModelList(OutfitFile[] maps)
        {
            var models = new List<Model>();

            //outfit models
            models.AddRange(GenerateOutfitList(maps));
            
            var file = await FileManager.ExtractFile(IoC.Decima,
                IoC.Config.TempPath, Configs.GamePackDir, false, @"entities/dlc1/characters/humanoids/uniquecharacters/dlc1_ikrie.core");

            var objs = CoreBinary.Load(file.Output);
            var resources = objs.Select(CoreNode.FromObj).Where(x => x.Type.Name == "HumanoidBodyVariant");

            models.AddRange(resources.Select(x =>
                new Model()
                {
                    Name = x.Name,
                    Id = x.Id
                }));

            return models;
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

            var maps = files.Select(x => GetOutfitMap(x.Source, x.Output)).ToArray();
            //await SaveOutfitMaps(maps);

            await FileManager.Cleanup(IoC.Config.TempPath);

            return maps;
        }

        private OutfitFile GetOutfitMap(string fileKey, string path)
        {
            var map = new OutfitFile()
            {
                File = fileKey
            };

            var objs = CoreBinary.Load(path);

            foreach (var item in GetOutfits(objs))
                map.Outfits.Add(item);

            return map;
        }

        private IEnumerable<Outfit> GetOutfits(List<object> nodes)
        {
            var items = GetTypes<InventoryEntityResource>(nodes).Values;
            var itemComponents = GetTypes<InventoryItemComponentResource>(nodes);
            var componentResources = GetTypes<NodeGraphComponentResource>(nodes);
            var overrides = GetTypes< OverrideGraphProgramResource>(nodes);
            var mappings = GetTypes< NodeGraphHumanoidBodyVariantUUIDRefVariableOverride>(nodes);

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
                                outfit.Id = mapItem.Object.GUID;
                                outfit.RefId = mapItem.ObjectUUID;
                                break;
                            }
                        }
                    }
                }

                yield return outfit;
            }
        }

        private Dictionary<BaseGGUUID, T> GetTypes<T>(List<object> nodes, string typeName = null) where T : RTTIRefObject
        {
            typeName = typeName ?? typeof(T).Name;

            return nodes.Where(x => x.GetType().Name == typeName)
                .ToDictionary(x => (BaseGGUUID)((T)x).ObjectUUID, x => (T)x);
        }

        public async Task<string> GeneratePatch(OutfitFile[] maps)
        {
            await FileManager.Cleanup(IoC.Config.TempPath);
            
            var patchDir = Path.Combine(IoC.Config.TempPath, PatchTempDir);

            foreach (var map in maps)
            {
                //extract game files to temp
                var file = await FileManager.ExtractFile(
                    IoC.Decima, patchDir, Configs.GamePackDir, true, map.File);

                if (file.Output == null)
                    throw new HzdException($"Unable to find file for map: {map.File}");

                var refs = map.Outfits.ToDictionary(x => x.RefId, x => x.Id);

                //update references from 
                var objs = CoreBinary.Load(file.Output);
                foreach (var reference in GetOutfitReferences(objs))
                {
                    if (refs.TryGetValue(reference.RefId, out var newModel))
                        reference.ModelId.AssignFromOther(newModel);
                }

                CoreBinary.Save(file.Output, objs);
            }

            //await AddCharacterReferences(patchDir);

            var output = Path.Combine(IoC.Config.TempPath, IoC.Config.PatchFile);

            await IoC.Decima.PackFiles(patchDir, output);

            return output;

            //await FileManager.Cleanup(IoC.Config.TempPath);
        }

        private IEnumerable<(BaseGGUUID ModelId, BaseGGUUID RefId)> GetOutfitReferences(List<object> components)
        {
            //NodeGraphHumanoidBodyVariantUUIDRefVariableOverride
            GetOutfits(components).ToList();
            var mappings = components.Select(CoreNode.FromObj).Where(x => x.Type.Name == "NodeGraphHumanoidBodyVariantUUIDRefVariableOverride");
            return mappings.Select((x, i) =>
            (
                CoreNode.FromObj(x.GetField<object>("Object"))?.GetField<BaseGGUUID>("GUID"),
                x.Id
            ));
        }

        //private async Task AddCharacterReferences(string path)
        //{
        //    var components = await LoadPlayerComponents(path, true);
        //    var outfits = GetVariants(components.Objects);

        //    var models = await GenerateModelList();
        //    var id = models.First(x => x.Name == "DLC1_Ikrie");
        //    //var id = models[3];

        //    //var sRef = (StreamingRef<HumanoidBodyVariant>)outfits[33];

        //    //sRef.ExternalFile = new BaseString("entities/characters/humanoids/player/costumes/playercostume_norastealth_heavy");

        //    var sRef = new StreamingRef<HumanoidBodyVariant>();
        //    sRef.ExternalFile = new BaseString("entities/dlc1/characters/humanoids/uniquecharacters/dlc1_ikrie");
        //    sRef.Type = BaseRef<HumanoidBodyVariant>.Types.StreamingRef;
        //    sRef.GUID = BaseGGUUID.FromOther(id.Id);

        //    outfits.Add(sRef);

        //    CoreBinary.Save(components.File, components.Objects);
        //}

        public async Task InstallPatch(string path)
        {
            await FileManager.InstallPatch(path, Configs.GamePackDir);
        }

        public bool CheckGameDir()
        {
            return Directory.Exists(Configs.GamePackDir);
        }
    }
}
