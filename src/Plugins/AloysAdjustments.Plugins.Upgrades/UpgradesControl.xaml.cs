using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.Upgrades.Data;
using AloysAdjustments.UI;
using Decima;

namespace AloysAdjustments.Plugins.Upgrades
{
    /// <summary>
    /// Interaction logic for UpgradeControl.xaml
    /// </summary>
    public partial class UpgradesControl : InteractivePluginControl, INotifyPropertyChanged
    {
        private UpgradesLogic Logic { get; }
        public List<Upgrade> Upgrades { get; set; }

        public override string PluginName => "Upgrades";

        public UpgradesControl()
        {
            IoC.Bind(Configs.LoadModuleConfig<UpgradeConfig>(PluginName));

            Logic = new UpgradesLogic();

            InitializeComponent();
        }

        public override async Task LoadPatch(string path)
        {
            await Initialize();

            IoC.Notif.ShowStatus("Loading upgrades...");

            var loadedUpgrades = await Logic.GenerateUpgradeList(f =>
                IoC.Archiver.LoadFileAsync(path, f));

            foreach (var upgrade in Upgrades)
            {
                if (loadedUpgrades.TryGetValue(upgrade.Id, out var loadedUpgrade))
                    upgrade.Value = loadedUpgrade.Value;
            }
        }

        public override void ApplyChanges(Patch patch)
        {
            if (Upgrades.Any(x => x.Modified))
                Logic.CreatePatch(patch, Upgrades);
        }

        public override async Task Initialize()
        {
            ResetSelected.Enabled = false;

            IoC.Notif.ShowStatus("Loading upgrades list...");
            Upgrades = (await Logic.GenerateUpgradeList(IoC.Archiver.LoadGameFileAsync))
                .Values.OrderBy(x => x.DisplayName).ToList();

            await UpdateDisplayNames(Upgrades);
        }

        protected override async Task Reset_Click()
        {
            using var _ = new ControlLock(Reset);

            await Initialize();

            IoC.Notif.ShowStatus("Reset complete");
        }

        protected override Task ResetSelected_Click()
        {
            if (dgUpgrades.SelectedItems.Count > 0)
            {
                foreach (var upgrade in dgUpgrades.SelectedItems.Cast<Upgrade>())
                    upgrade.Value = upgrade.DefaultValue;
            }

            return Task.CompletedTask;
        }

        public async Task UpdateDisplayNames(IEnumerable<Upgrade> upgrades)
        {
            foreach (var o in upgrades)
            {
                o.SetDisplayName(await IoC.Localization.GetString(o.LocalNameFile, o.LocalNameId));
            }
        }

        private void btnMulti2_Click(object sender, RoutedEventArgs e) => MultiplyValues(2);
        private void btnMulti5_Click(object sender, RoutedEventArgs e) => MultiplyValues(5);
        private void btnMulti10_Click(object sender, RoutedEventArgs e) => MultiplyValues(10);

        private void MultiplyValues(int multi)
        {
            Upgrades.ForEach(x => {
                if ((long)x.Value * multi > int.MaxValue)
                    x.Value = int.MaxValue;
                else
                    x.Value *= multi;
            });
        }

        private void dgUpgrades_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetSelected.Enabled = e.AddedItems.Count > 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
