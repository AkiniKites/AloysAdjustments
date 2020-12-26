using Decima;
using HZDUtility.Utility;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HZDUtility
{
    public partial class Form1 : Form
    {
        private const string ConfigPath = "config.json";

        private Config Config { get; set; }

        public Form1()
        {
            InitializeComponent();

            RTTI.SetGameMode(GameType.HZD);
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

        public void Switch()
        {
            /*
            
            find and replace uuid in all:            
            
            
            NodeGraphHumanoidBodyVariantInterfaceUUIDRefVariableOverride
            Object GUID

            */
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
                item.Outfit.AssignFromOther(armors[0].Id);

            CoreBinary.Save(Path.Combine(Config.GamePath, Config.OutfitFiles[1]+".2"), objs2);
        }

        public void LoadOutfitList()
        {
            var objs = CoreBinary.Load(Path.Combine(@"e:\hzd\", Config.PlayerComponentsFile));
            var armors = GetOutfitList(objs).ToList();

            foreach (var item in armors)
            {
                lbOutfits.Items.Add(item);
            }
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

        public OutfitMap[] LoadOutfitMaps()
        {
            var json = File.ReadAllText(Config.OutfitMapPath);

            return JsonConvert.DeserializeObject<OutfitMap[]>(json, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                Converters = new[] { new BaseGGUUIDConverter() }
            });
        }

        public async Task GenerateOutfitMaps(string outfitPath)
        {
            var files = await FileManager.ExtractFiles(Config.DecimaPath, Config.TempPath, Config.GamePath, Config.OutfitFiles);

            var maps = files.Select(x => GetOutfitMap(x.Key, x.Value)).ToArray();
            await SaveOutfitMaps(outfitPath, maps);

            await FileManager.Cleanup(Config.TempPath);
        }

        public async Task SaveOutfitMaps(string outfitPath, OutfitMap[] maps)
        {
            var json = JsonConvert.SerializeObject(maps, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                Converters = new[] { new BaseGGUUIDConverter() }
            });

            await File.WriteAllTextAsync(outfitPath, json);
        }

        public OutfitMap GetOutfitMap(string fileKey, string path)
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

        public IEnumerable<(BaseGGUUID Outfit, BaseGGUUID RefId)> GetOutfitReferences(List<object> components)
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

        private void btnUpdateDefaultMaps_Click(object sender, EventArgs e)
        {
            GenerateOutfitMaps(Config.OutfitMapPath);
        }

        private void lbxArmors_SelectedValueChanged(object sender, EventArgs e)
        {

        }

        public void SetStatus(string text)
        {
            this.TryBeginInvoke(() => tssStatus.Text = text);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            SetStatus("Loading config");
            await LoadConfig();

            SetStatus("Checking outfit maps");
            if (!File.Exists(Config.OutfitMapPath))
            {
                SetStatus("Generating outfit maps");
                await GenerateOutfitMaps(Config.OutfitMapPath);
            }

            LoadOutfitList();
            test();
        }
    }
}
