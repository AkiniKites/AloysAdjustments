using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Modules.Misc;
using AloysAdjustments.Plugins.Misc.Data;
using AloysAdjustments.WPF.Plugins;
using AloysAdjustments.WPF.UI;

namespace AloysAdjustments.Plugins.Misc
{
    /// <summary>
    /// Interaction logic for MiscControl.xaml
    /// </summary>
    public partial class MiscControl : InteractivePluginBase
    {
        private bool _loading;

        private MiscLogic MiscLogic { get; }
        private MiscAdjustments DefaultAdjustments { get; set; }
        private MiscAdjustments Adjustments { get; set; }

        public override string PluginName => "Misc";

        public MiscControl()
        {
            _loading = true;

            IoC.Bind(Configs.LoadModuleConfig<MiscConfig>(PluginName));

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

        private void cbIntroLogos_Checked(object sender, RoutedEventArgs e)
        {
            if (_loading) return;

            Adjustments.SkipIntroLogos = cbIntroLogos.IsChecked;
            RefreshControls();
        }

        private void RefreshControls()
        {
            _loading = true;

            cbIntroLogos.IsChecked = Adjustments.SkipIntroLogos == true;
            cbIntroLogos.Content = DefaultAdjustments.SkipIntroLogos == Adjustments.SkipIntroLogos
                ? "Remove Intro Logos" : "Remove Intro Logos *";

            _loading = false;
        }
    }
}
