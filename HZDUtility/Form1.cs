using Decima;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        public void Switch()
        {
            /*
            get armor list
            e:\hzd\entities\characters\humanoids\player\player_components.core
            bodyvariantcomponentresource > playerbodyvariants
            variants list
            
            find and replace uuid in all:            
            entities/armor/newgameplus/ng_outfits.core
            entities/armor/outfits/outfits.core
            entities/dlc1/outfits/dlc1_outfits.core
            
            NodeGraphHumanoidBodyVariantInterfaceUUIDRefVariableOverride
            Object GUID

            */
        }

        public void test()
        {
            var objs = CoreBinary.Load(@"e:\hzd\entities\characters\humanoids\player\player_components.core");
            var armors = GetArmorList(objs).ToList();
        }

        public IEnumerable<(string Name, string Id)> GetArmorList(List<object> components)
        {
            //bodyvariantcomponentresource
            var resource = components.Select(CoreNode.FromObj).FirstOrDefault(x => x.Name == "PlayerBodyVariants");
            if (resource == null)
                throw new Exception("Unable to find BodyVariantComponentResource");

            var armors = resource.GetField<IList>("Variants");
            return armors.Cast<object>().Select(CoreNode.FromObj).Select(x =>
                (
                    x.GetString("ExternalFile"),
                    x.GetString("GUID")
                ));
        }
    }
}
