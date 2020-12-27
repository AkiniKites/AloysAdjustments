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
    public partial class Main : Form
    {
        private const string ConfigPath = "config.json";
        private Logic Logic { get; set; }

        public Main()
        {
            InitializeComponent();

            lbOutfits.DrawMode = DrawMode.OwnerDrawVariable;
            lbOutfits.ItemHeight = lbOutfits.Font.Height + 2;
            lbOutfits.DrawItem += (s, e) =>
            {
                var l = (ListBox)s;

                e.DrawBackground();
                e.Graphics.DrawString(l.Items[e.Index].ToString(),
                    e.Font, new SolidBrush(e.ForeColor), e.Bounds, StringFormat.GenericDefault);
                e.DrawFocusRectangle();
            };

            RTTI.SetGameMode(GameType.HZD);
        }

        private async void btnUpdateDefaultMaps_Click(object sender, EventArgs e)
        {
            SetStatus("Generating outfit maps");
            await Logic.GenerateOutfitMaps();

            SetStatus("");
        }

        private void lbOutfits_SelectedValueChanged(object sender, EventArgs e)
        {

        }

        public void SetStatus(string text)
        {
            this.TryBeginInvoke(() => tssStatus.Text = text);
        }

        private async void Main_Load(object sender, EventArgs e)
        {
            SetStatus("Loading config");
            Logic = await Logic.FromConfig(ConfigPath);

            SetStatus("Checking outfit maps");
            //if (!Logic.HasOutfitMap())
            {
                SetStatus("Generating outfit maps");
                await Logic.GenerateOutfitMaps();
            }

            SetStatus("Loading outfit list");
            var outfits = Logic.LoadOutfitList();
            foreach (var item in outfits)
                lbOutfits.Items.Add(item);

            SetStatus("Loading models list");
            var models = Logic.LoadModelList();
            foreach (var item in models)
                clbModels.Items.Add(item);

            //test();

            SetStatus("");
        }
    }
}
