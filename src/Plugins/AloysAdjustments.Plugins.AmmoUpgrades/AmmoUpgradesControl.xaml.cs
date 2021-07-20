using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.AmmoUpgrades.Data;
using AloysAdjustments.Plugins.Upgrades;
using AloysAdjustments.UI;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Plugins.AmmoUpgrades
{
    /// <summary>
    /// Interaction logic for UpgradeControl.xaml
    /// </summary>
    public partial class AmmoUpgradesControl : InteractivePluginControl, INotifyPropertyChanged
    {
        private AmmoUpgradesLogic Logic { get; }
        public ListCollectionView UpgradesView { get; set; }
        public List<AmmoUpgrade> Upgrades { get; set; }

        public override string PluginName => "Ammo Upgrades";

        public AmmoUpgradesControl()
        {
            IoC.Bind(Configs.LoadModuleConfig<AmmoUpgradeConfig>(PluginName));

            Logic = new AmmoUpgradesLogic();

            Upgrades = new List<AmmoUpgrade>();
            UpgradesView = CollectionViewSource.GetDefaultView(Upgrades) as ListCollectionView;
            
            InitializeComponent();

            dgUpgrades.SetDefaultSorts(new [] {
                new SortDescription("Sort", ListSortDirection.Ascending),
                new SortDescription("DisplayName", ListSortDirection.Ascending),
                new SortDescription("Level", ListSortDirection.Ascending)
            });
        }

        public override async Task LoadPatch(string path)
        {
            await Initialize();

            IoC.Notif.ShowStatus("Loading upgrades...");

            var loadedUpgrades = await Logic.GenerateUpgradeList(f =>
                IoC.Archiver.LoadFileAsync(path, f));

            foreach (var upgrade in Upgrades)
            {
                if (loadedUpgrades.TryGetValue((upgrade.Id, upgrade.Level), out var loadedUpgrade))
                    upgrade.Value = loadedUpgrade.Value;
            }
        }

        public override void ApplyChanges(Patch patch)
        {
            if (Upgrades.Any(x => x.Modified))
                Logic.CreatePatch(patch, Upgrades);
        }

        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        public override async Task Initialize()
        {
            await _lock.WaitAsync();
            {
                ResetSelected.Enabled = false;

                IoC.Notif.ShowStatus("Loading upgrades list...");

                Upgrades.Clear();


                var upgrades = (await Logic.GenerateUpgradeList(IoC.Archiver.LoadGameFileAsync)).Values
                    .OrderBy(x => x.Level).ToList();
                Upgrades.AddRange(upgrades);

                await UpdateDisplayNames(Upgrades);

                RefreshView();
            }
            _lock.Release();
        }

        private void RefreshView()
        {
            UpgradesView.Refresh();
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
                foreach (var upgrade in dgUpgrades.SelectedItems.Cast<AmmoUpgrade>())
                    upgrade.Value = upgrade.DefaultValue;
            }

            return Task.CompletedTask;
        }

        public async Task UpdateDisplayNames(IEnumerable<AmmoUpgrade> upgrades)
        {
            foreach (var upgrade in upgrades)
            {
                async Task<string> GetLocal(LocalString localText)
                {
                    var text = await IoC.Localization.GetString(localText);
                    text = IoC.Localization.ToTitleCase(text);
                    if (upgrade.Level == 0)
                        text = text.Replace("1", "0");
                    return text;
                }

                upgrade.SetDisplayName(await GetLocal(upgrade.LocalName));
                upgrade.DisplayCategory = await GetLocal(upgrade.LocalCategory);
            }
        }

        private void btnMulti2_Click(object sender, RoutedEventArgs e) => MultiplyValues(2);
        private void btnMulti5_Click(object sender, RoutedEventArgs e) => MultiplyValues(5);
        private void btnMulti10_Click(object sender, RoutedEventArgs e) => MultiplyValues(10);

        private void MultiplyValues(int multi)
        {
            var upgrades = dgUpgrades.SelectedItems.Count == 0 ?
                Upgrades : dgUpgrades.SelectedItems.Cast<AmmoUpgrade>();

            foreach (var upgrade in upgrades)
            {
                if ((long)upgrade.Value * multi > int.MaxValue)
                    upgrade.Value = int.MaxValue;
                else
                    upgrade.Value *= multi;
            }
        }

        private void dgUpgrades_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetSelected.Enabled = e.AddedItems.Count > 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
