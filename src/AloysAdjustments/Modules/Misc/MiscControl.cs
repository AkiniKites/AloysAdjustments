using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Configuration;
using AloysAdjustments.Data;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Modules.Misc
{
    public partial class MiscControl : ModuleBase
    {
        private bool _loading;

        private MiscLogic MiscLogic { get; }
        private MiscAdjustments DefaultAdjustments { get; set; }
        private MiscAdjustments Adjustments { get; set; }
        
        public override string ModuleName => "Misc";

        public MiscControl()
        {
            _loading = true;

            IoC.Bind(Configs.LoadModuleConfig<MiscConfig>(ModuleName));

            MiscLogic = new MiscLogic();

            InitializeComponent();

            _loading = false;
        }

        public override async Task Initialize()
        {
            IoC.Notif.ShowUnknownProgress();

            IoC.Notif.ShowStatus("Loading misc data...");
            DefaultAdjustments = await MiscLogic.GenerateMiscData(
                IoC.Archiver.LoadGameFileAsync);
            Adjustments = DefaultAdjustments.Clone();

            RefreshControls();
        }

        public override async Task LoadPatch(string path)
        {
            await Initialize();

            IoC.Notif.ShowStatus("Loading misc data...");
            var newAdj = await MiscLogic.GenerateMiscData(f =>
                IoC.Archiver.LoadFileAsync(path, f));

            if (newAdj.SkipIntroLogos.HasValue) Adjustments.SkipIntroLogos = newAdj.SkipIntroLogos;

            RefreshControls();
        }

        public override void ApplyChanges(Patch patch)
        {
            var changedValues = Adjustments.Clone();

            if (Adjustments.SkipIntroLogos == DefaultAdjustments.SkipIntroLogos)
                changedValues.SkipIntroLogos = null;

            MiscLogic.CreatePatch(patch, Adjustments);
        }


        private async Task Reload()
        {
            await Initialize();

            IoC.Notif.HideProgress();
        }

        protected override async Task Reset_Click()
        {
            using var _ = new ControlLock(Reset);

            await Reload();
            IoC.Notif.ShowStatus("Reset complete");
        }
        
        private void cbIntroLogos_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            Adjustments.SkipIntroLogos = cbIntroLogos.Checked;
            RefreshControls();
        }

        private void RefreshControls()
        {
            _loading = true;

            cbIntroLogos.Checked = Adjustments.SkipIntroLogos == true;
            cbIntroLogos.Text = DefaultAdjustments.SkipIntroLogos == Adjustments.SkipIntroLogos 
                ? "Remove Intro Logos" : "Remove Intro Logos *";

            _loading = false;
        }
    }
}
