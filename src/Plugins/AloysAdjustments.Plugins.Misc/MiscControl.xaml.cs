using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.Misc.Data;
using AloysAdjustments.UI;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Plugins.Misc
{
    /// <summary>
    /// Interaction logic for MiscControl.xaml
    /// </summary>
    public partial class MiscControl : InteractivePluginControl
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
            ResetSelected.Enabled = false;

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
            if (newAdj.RemoveMenuMusic.HasValue) Adjustments.RemoveMenuMusic = newAdj.RemoveMenuMusic;

            RefreshControls();
        }

        public override void ApplyChanges(Patch patch)
        {
            var changedValues = Adjustments.Clone();

            if (Adjustments.SkipIntroLogos == DefaultAdjustments.SkipIntroLogos)
                changedValues.SkipIntroLogos = null;
            if (Adjustments.RemoveMenuMusic == DefaultAdjustments.RemoveMenuMusic)
                changedValues.RemoveMenuMusic = null;

            MiscLogic.CreatePatch(patch, Adjustments);
        }

        public override Task CommandAction(string command)
        {
            //skip intro movies: i
            if (command.IndexOf("i", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Adjustments.SkipIntroLogos = true;
            }

            //mute menu music: m
            if (command.IndexOf("m", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Adjustments.RemoveMenuMusic = true;
            }

            return Task.CompletedTask;
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

        private void cbMenuMusic_Checked(object sender, RoutedEventArgs e)
        {
            if (_loading) return;

            Adjustments.RemoveMenuMusic = cbMenuMusic.IsChecked;
            RefreshControls();
        }

        private void RefreshControls()
        {
            _loading = true;

            cbIntroLogos.IsChecked = Adjustments.SkipIntroLogos == true;
            cbIntroLogos.Content = DefaultAdjustments.SkipIntroLogos == Adjustments.SkipIntroLogos
                ? "Remove intro logos" : "Remove intro logos *";

            cbMenuMusic.IsChecked = Adjustments.RemoveMenuMusic == true;
            cbMenuMusic.Content = DefaultAdjustments.RemoveMenuMusic == Adjustments.RemoveMenuMusic
                ? "Remove in-game menu music" : "Remove in-game menu music *";

            _loading = false;
        }
    }
}
