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

namespace HZDUtility
{
    public class Logic
    {
        private string ConfigPath { get; set; }

        public Config Config { get; set; }

        private Logic() { }

        public static async Task<Logic> FromConfig(string configPath)
        {
            var l = new Logic()
            {
                ConfigPath = configPath
            };

            await l.LoadConfig();
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

        public void test()
        {
            //entities\characters\humanoids\player\player_components.core
            var objs = CoreBinary.Load(@"e:\hzd\entities\characters\humanoids\player\player_components.core");
            var armors = GetOutfitList(objs).ToList();

            foreach (var item in armors)
            {
                Debug.WriteLine(item);
            }

            Debug.WriteLine("\r\n\r\n\r\n");

            var maps = LoadOutfitMaps();

            var objs2 = CoreBinary.Load(Path.Combine(Config.GamePath, Config.OutfitFiles[1]));

            foreach (var item in GetOutfitReferences(objs2))
                item.ModelId.AssignFromOther(armors[0].Id);

            CoreBinary.Save(Path.Combine(Config.GamePath, Config.OutfitFiles[1] + ".2"), objs2);
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
                throw new Exception("Unable to find PlayerBodyVariants");

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
            var files = await FileManager.ExtractFiles(Config.DecimaPath, 
                Config.TempPath, Config.GamePath, false, Config.OutfitFiles);

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

        public async Task GeneratePatch(OutfitMap[] maps)
        {
            await FileManager.Cleanup(Config.TempPath);

            foreach (var map in maps)
            {
                //extract game files to temp
                var file = (await FileManager.ExtractFiles(
                    Config.DecimaPath, Config.TempPath, Config.GamePath, true, map.File))
                    .FirstOrDefault();

                if (file.Key == null)
                    throw new Exception($"Unable to find file for map: {map.File}");

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

            var output = Path.Combine(Config.TempPath, "zMod_OutfitSwap.bin");

            await FileManager.PackFiles(Config.DecimaPath, Config.TempPath, output);
            
            //await FileManager.Cleanup(Config.TempPath);
        }
    }
}
