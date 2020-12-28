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
using Decima.DS;
using HZDUtility.Models;
using HZDUtility.Utility;
using Model = HZDUtility.Models.Model;

namespace HZDUtility
{
    public class Logic
    {
        private string ConfigPath { get; set; }

        public Config Config { get; private set; }
        public Decima Decima { get; private set; }

        private Logic() { }

        public static async Task<Logic> FromConfig(string configPath)
        {
            var l = new Logic()
            {
                ConfigPath = configPath
            };
            
            await l.LoadConfig();
            l.Decima = new Decima(l.Config);

            return l;
        }

        public async Task LoadConfig()
        {
            var json = await File.ReadAllTextAsync(ConfigPath);
            Config = await Task.Run(() => JsonConvert.DeserializeObject<Config>(json));
        }

        public async Task SaveConfig()
        {
            var json = JsonConvert.SerializeObject(Config, Formatting.Indented);
            await File.WriteAllTextAsync(ConfigPath, json);
        }

        public async Task<List<Outfit>> LoadOutfitList()
        {
            var packDir = Path.Combine(Config.Settings.GamePath, Config.PackDir);
            var file = await FileManager.ExtractFile(Decima,
                Config.TempPath, packDir, false, Config.PlayerComponentsFile);
            
            if (file.Output == null)
                throw new HzdException($"Unable to find {Config.PlayerComponentsFile}");

            var objs = CoreBinary.Load(file.Output);
            var outfits = GetOutfitList(objs).ToList();

            return outfits;
        }

        public IEnumerable<Outfit> GetOutfitList(List<object> components)
        {
            //BodyVariantComponentResource
            var resource = components.Select(CoreNode.FromObj).FirstOrDefault(x => x.Name == "PlayerBodyVariants");
            if (resource == null)
                throw new HzdException("Unable to find PlayerBodyVariants");

            var outfits = resource.GetField<IList>("Variants");
            return outfits.Cast<object>().Select(CoreNode.FromObj).Select(x =>
                new Outfit()
                {
                    Name = x.GetString("ExternalFile"),
                    Id = x.GetField<BaseGGUUID>("GUID")
                });
        }


        public async Task<List<Model>> LoadModelList()
        {
            var models = new List<Model>();

            //outfit models
            models.AddRange(await LoadOutfitList());

            return models;
        }

        public bool HasOutfitMap()
        {
            return File.Exists(Config.OutfitMapPath);
        }

        public async Task<OutfitMap[]> LoadOutfitMaps()
        {
            var json = await File.ReadAllTextAsync(Config.OutfitMapPath);

            return JsonConvert.DeserializeObject<OutfitMap[]>(json, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                Converters = new[] { new BaseGGUUIDConverter() }
            });
        }

        public async Task SaveOutfitMaps(OutfitMap[] maps)
        {
            var json = JsonConvert.SerializeObject(maps, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                Converters = new[] { new BaseGGUUIDConverter() }
            });

            await File.WriteAllTextAsync(Config.OutfitMapPath, json);
        }

        public async Task<OutfitMap[]> GenerateOutfitMaps()
        {
            //extract game files
            var packDir = Path.Combine(Config.Settings.GamePath, Config.PackDir);

            //TODO: Fix hack to ignore patch files
            var patch = Path.Combine(packDir, Config.PatchFile);
            var patchTemp = patch + Guid.NewGuid();
            if (File.Exists(patch))
                File.Move(patch, patchTemp);

            var maps = await GenerateOutfitMapsFromPack(packDir);

            if (File.Exists(patchTemp))
                File.Move(patchTemp, patch);

            return maps;
        }
        public async Task<OutfitMap[]> GenerateOutfitMapsFromPack(string path)
        {
            var files = await FileManager.ExtractFiles(Decima, 
                Config.TempPath, path, false, Config.OutfitFiles);

            var maps = files.Select(x => GetOutfitMap(x.Source, x.Output)).ToArray();
            await SaveOutfitMaps(maps);

            await FileManager.Cleanup(Config.TempPath);

            return maps;
        }

        private OutfitMap GetOutfitMap(string fileKey, string path)
        {
            var map = new OutfitMap()
            {
                File = fileKey
            };

            var objs = CoreBinary.Load(path);

            foreach (var item in GetOutfitReferences(objs))
                map.Refs.Add(item);

            return map;
        }

        private IEnumerable<(BaseGGUUID ModelId, BaseGGUUID RefId)> GetOutfitReferences(List<object> components)
        {
            //NodeGraphHumanoidBodyVariantUUIDRefVariableOverride
            var mappings = components.Select(CoreNode.FromObj).Where(x => x.Type.Name == "NodeGraphHumanoidBodyVariantUUIDRefVariableOverride");

            return mappings.Select(x =>
                (
                    CoreNode.FromObj(x.GetField<object>("Object"))?.GetField<BaseGGUUID>("GUID"),
                    x.Id
                ));
        }

        public async Task<string> GeneratePatch(OutfitMap[] maps)
        {
            await FileManager.Cleanup(Config.TempPath);

            var packDir = Path.Combine(Config.Settings.GamePath, Config.PackDir);

            foreach (var map in maps)
            {
                //extract game files to temp
                var file = await FileManager.ExtractFile(
                    Decima, Config.TempPath, packDir, true, map.File);

                if (file.Output == null)
                    throw new HzdException($"Unable to find file for map: {map.File}");

                var refs = map.Refs.ToDictionary(x => x.RefId, x => x.ModelId);

                //update references from 
                var objs = CoreBinary.Load(file.Output);
                foreach (var reference in GetOutfitReferences(objs))
                {
                    if (refs.TryGetValue(reference.RefId, out var newModel))
                        reference.ModelId.AssignFromOther(newModel);
                }

                CoreBinary.Save(file.Output, objs);
            }

            var output = Path.Combine(Config.TempPath, Config.PatchFile);

            await Decima.PackFiles(Config.TempPath, output);

            return output;

            //await FileManager.Cleanup(Config.TempPath);
        }

        public async Task InstallPatch(string path)
        {
            var packDir = Path.Combine(Config.Settings.GamePath, Config.PackDir);
            await FileManager.InstallPatch(path, packDir);
        }

        public bool CheckGameDir()
        {
            if (!Directory.Exists(Config.Settings.GamePath))
                return false;
            return Directory.Exists(Path.Combine(Config.Settings.GamePath, Config.PackDir));
        }
    }
}
