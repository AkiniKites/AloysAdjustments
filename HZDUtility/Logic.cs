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

        public void SaveConfig()
        {
            var json = JsonConvert.SerializeObject(Config, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
        }

        public List<Outfit> LoadOutfitList()
        {
            var objs = CoreBinary.Load(Path.Combine(@"e:\hzd\", Config.PlayerComponentsFile));
            var armors = GetOutfitList(objs).ToList();

            return armors;
        }

        public IEnumerable<Outfit> GetOutfitList(List<object> components)
        {
            //BodyVariantComponentResource
            var resource = components.Select(CoreNode.FromObj).FirstOrDefault(x => x.Name == "PlayerBodyVariants");
            if (resource == null)
                throw new HzdException("Unable to find PlayerBodyVariants");

            var armors = resource.GetField<IList>("Variants");
            return armors.Cast<object>().Select(CoreNode.FromObj).Select(x =>
                new Outfit()
                {
                    Name = x.GetString("ExternalFile"),
                    Id = x.GetField<BaseGGUUID>("GUID")
                });
        }


        public List<Model> LoadModelList()
        {
            var objs = CoreBinary.Load(Path.Combine(@"e:\hzd\", Config.PlayerComponentsFile));
            var models = new List<Model>();

            models.AddRange(GetOutfitList(objs));

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
            //extract game files to temp
            var packDir = Path.Combine(Config.GamePath, Config.PackDir);
            var files = await FileManager.ExtractFiles(Decima, 
                Config.TempPath, packDir, false, Config.OutfitFiles);

            var maps = files.Select(x => GetOutfitMap(x.Key, x.Value)).ToArray();
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
            var name = components.Select(CoreNode.FromObj).FirstOrDefault().Type.Name;
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

            var packDir = Path.Combine(Config.GamePath, Config.PackDir);

            foreach (var map in maps)
            {
                //extract game files to temp
                var file = (await FileManager.ExtractFiles(
                        Decima, Config.TempPath, packDir, true, map.File))
                    .FirstOrDefault();

                if (file.Key == null)
                    throw new HzdException($"Unable to find file for map: {map.File}");

                var refs = map.Refs.ToDictionary(x => x.RefId, x => x.ModelId);

                //update references from 
                var objs = CoreBinary.Load(file.Value);
                foreach (var reference in GetOutfitReferences(objs))
                {
                    if (refs.TryGetValue(reference.RefId, out var newModel))
                        reference.ModelId.AssignFromOther(newModel);
                }

                CoreBinary.Save(file.Value, objs);
            }

            var output = Path.Combine(Config.TempPath, Config.PatchFile);

            await Decima.PackFiles(Config.TempPath, output);

            return output;

            //await FileManager.Cleanup(Config.TempPath);
        }

        public async Task InstallPatch(string path)
        {
            var packDir = Path.Combine(Config.GamePath, Config.PackDir);
            await FileManager.InstallPatch(path, packDir);
        }
    }
}
