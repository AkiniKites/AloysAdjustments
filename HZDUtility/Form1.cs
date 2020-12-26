using Decima;
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
        public Form1()
        {
            InitializeComponent();

            RTTI.SetGameMode(GameType.HZD);
            test();
        }

        public static string[] OutfitFiles = new[]
        {
            "entities/armor/newgameplus/ng_outfits.core",
            "entities/armor/outfits/outfits.core",
            "entities/dlc1/outfits/dlc1_outfits.core"
        };

        public static string OutfitMapPath = "outfit-map.json";
        public static string GamePath = @"E:\Games\Horizon Zero Dawn Mods\Armor Change";

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
            var armors = GetArmorList(objs).ToList();

            foreach (var item in armors)
            {
                Debug.WriteLine(item);
            }

            Debug.WriteLine("\r\n\r\n\r\n");

            GenerateOutfitMaps();
            LoadOutfitMaps();
        }

        public IEnumerable<(string Name, BaseGGUUID Id)> GetArmorList(List<object> components)
        {
            //BodyVariantComponentResource
            var resource = components.Select(CoreNode.FromObj).FirstOrDefault(x => x.Name == "PlayerBodyVariants");
            if (resource == null)
                throw new Exception("Unable to find PlayerBodyVariants");

            var armors = resource.GetField<IList>("Variants");
            return armors.Cast<object>().Select(CoreNode.FromObj).Select(x =>
                (
                    x.GetString("ExternalFile"),
                    x.GetField<BaseGGUUID>("GUID")
                ));
        }

        public OutfitMap[] LoadOutfitMaps()
        {
            var json = File.ReadAllText(OutfitMapPath);

            return JsonConvert.DeserializeObject<OutfitMap[]>(json, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                Converters = new[] { new BaseGGUUIDConverter() }
            });
        }

        public void GenerateOutfitMaps()
        {
            var maps = OutfitFiles.Select(x => GetOutfitMap(GamePath, x)).ToArray();

            var json = JsonConvert.SerializeObject(maps, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                Converters = new[] { new BaseGGUUIDConverter() }
            });

            File.WriteAllText(OutfitMapPath, json);
        }

        public OutfitMap GetOutfitMap(string gamePath, string file)
        {
            var map = new OutfitMap()
            {
                File = file
            };

            var objs = CoreBinary.Load(Path.Combine(gamePath, file));

            foreach (var item in GetArmorReferences(objs))
                map.Refs.Add(item);

            return map;
        }

        public IEnumerable<(BaseGGUUID Ref, BaseGGUUID Armor)> GetArmorReferences(List<object> components)
        {
            //NodeGraphHumanoidBodyVariantUUIDRefVariableOverride
            var name = components.Select(CoreNode.FromObj).FirstOrDefault().Type.Name;
            var mappings = components.Select(CoreNode.FromObj).Where(x => x.Type.Name == "NodeGraphHumanoidBodyVariantUUIDRefVariableOverride");

            return mappings.Select(x =>
                (
                    x.Id,
                    CoreNode.FromObj(x.GetField<object>("Object"))?.GetField<BaseGGUUID>("GUID")
                ));
        }
    }
}
