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
using AloysAdjustments.Utility;
using Decima;

namespace AloysAdjustments.Modules.Upgrades
{
    public partial class UpgradesControl : ModuleBase
    {
        private UpgradesLogic Logic { get; }
        private Dictionary<BaseGGUUID, Upgrade> DefaultUpgrades { get; set; }
        private List<Upgrade> NewUpgrades { get; set; }

        public override string ModuleName => "Upgrades";

        public UpgradesControl()
        {
            IoC.Bind(Configs.LoadModuleConfig<UpgradeConfig>(ModuleName));

            Logic = new UpgradesLogic();

            InitializeComponent();

            SetupDataGrid();
        }

        private void SetupDataGrid()
        {
            dgvUpgrades.AutoGenerateColumns = false;

            dgvUpgrades.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "Upgrade",
                DataPropertyName = "DisplayName",
                ReadOnly = true,
                Width = 220
            });
            dgvUpgrades.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "Capacity Increase",
                DataPropertyName = "Value",
                Width = 130
            });
            //dummy
            dgvUpgrades.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvUpgrades.CellValidating += (s, e) =>
            {
                if (e.ColumnIndex != 1)
                    return;

                if (!int.TryParse(Convert.ToString(e.FormattedValue), out var i) || i < 0)
                {
                    e.Cancel = true;
                }
            };

            dgvUpgrades.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < NewUpgrades.Count)
                {
                    var upgrade = NewUpgrades[e.RowIndex];
                    if (upgrade.Modified)
                        e.CellStyle.BackColor = Color.LightSkyBlue;
                }
            };
        }

        public override async Task LoadPatch(string path)
        {
            await Initialize();

            IoC.Notif.ShowStatus("Loading upgrades...");
            
            var loadedUpgrades = await Logic.GenerateUpgradeListFromPath(path, false);

            foreach (var upgrade in NewUpgrades)
            {
                if (loadedUpgrades.TryGetValue(upgrade.Id, out var loadedUpgrade))
                {
                    upgrade.Modified = upgrade.Value != loadedUpgrade.Value;
                    upgrade.Value = loadedUpgrade.Value;
                }
            }

            dgvUpgrades.Refresh();
        }

        public override async Task ApplyChanges(Patch patch)
        {
            foreach (var upgrade in NewUpgrades)
            {
                var defaultUpgrade = DefaultUpgrades[upgrade.Id];
                if (defaultUpgrade.Value != upgrade.Value)
                {
                    await Logic.CreatePatch(patch, NewUpgrades);
                    break;
                }
            }
        }

        public override async Task Initialize()
        {
            ResetSelected.Enabled = false;

            IoC.Notif.ShowStatus("Loading upgrades list...");
            DefaultUpgrades = await Logic.GenerateUpgradeList();
            NewUpgrades = DefaultUpgrades.Values
                .Select(x=>x.Clone())
                .OrderBy(x => x.DisplayName).ToList();

            await UpdateDisplayNames(NewUpgrades);
            dgvUpgrades.DataSource = NewUpgrades;
        }

        protected override async Task Reset_Click()
        {
            using var _ = new ControlLock(Reset);

            await Initialize();

            IoC.Notif.ShowStatus("Reset complete");
        }

        protected override Task ResetSelected_Click()
        {
            if (dgvUpgrades.SelectedRows.Count > 0)
            {
                foreach (var row in dgvUpgrades.SelectedRows.Cast<DataGridViewRow>())
                {
                    var upgrade = NewUpgrades[row.Index];
                    upgrade.Value = DefaultUpgrades[upgrade.Id].Value;
                    upgrade.Modified = false;
                }
            }

            dgvUpgrades.Refresh();

            return Task.CompletedTask;
        }

        public async Task UpdateDisplayNames(IEnumerable<Upgrade> upgrades)
        {
            foreach (var o in upgrades)
            {
                o.SetDisplayName(await IoC.Localization.GetString(o.LocalNameFile, o.LocalNameId));
            }
        }

        private void btnMulti2_Click(object sender, EventArgs e) => MultiplyValues(2);
        private void btnMulti5_Click(object sender, EventArgs e) => MultiplyValues(5);
        private void btnMulti10_Click(object sender, EventArgs e) => MultiplyValues(10);

        private void MultiplyValues(int multi)
        {
            NewUpgrades.ForEach(x => {
                if ((long)x.Value * multi > int.MaxValue)
                    x.Value = int.MaxValue;
                else
                    x.Value *= multi;

                UpdateModified(x);
            });
            dgvUpgrades.Refresh();
        }

        private void UpdateModified(Upgrade upgrade)
        {
            var orig = DefaultUpgrades[upgrade.Id];
            upgrade.Modified = orig.Value != upgrade.Value;
        }

        private void dgvUpgrades_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            UpdateModified(NewUpgrades[e.RowIndex]);
            dgvUpgrades.Refresh();
        }

        private void dgvUpgrades_SelectionChanged(object sender, EventArgs e)
        {
            ResetSelected.Enabled = dgvUpgrades.SelectedRows.Count > 0;
        }
    }
}
