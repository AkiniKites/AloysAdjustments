using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Data;
using AloysAdjustments.Logic;
using AloysAdjustments.Utility;
using Decima;

namespace AloysAdjustments.Modules
{
    public partial class UpgradesControl : UserControl, IModule
    {
        private bool _active;

        private UpgradesLogic Logic { get; set; }
        private Dictionary<BaseGGUUID, Upgrade> DefaultUpgrades { get; set; }
        private List<Upgrade> NewUpgrades { get; set; }

        public string ModuleName => "Upgrades";
        public Control ModuleControl => this;

        public Button Reset { get; set; }
        public Button ResetSelected { get; set; }

        public UpgradesControl()
        {
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
        
        public void Activate()
        {
            _active = true;
            Reset.Click += Reset_Click;
            ResetSelected.Click += ResetSelected_Click;

            ResetSelected.Enabled = dgvUpgrades.SelectedRows.Count > 0;
        }

        public void DeActivate()
        {
            _active = false;
            Reset.Click -= Reset_Click;
            ResetSelected.Click -= ResetSelected_Click;
        }

        public async Task LoadPatch(string path)
        {
            await Initialize();

            IoC.SetStatus("Loading upgrades...");
            
            var loadedUpgrades = await Logic.GenerateUpgradeListFromPath(path);

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

        public async Task CreatePatch(string patchDir)
        {
            await Logic.CreatePatch(patchDir, NewUpgrades);
        }

        public async Task Initialize()
        {
            ResetSelected.Enabled = false;

            IoC.SetStatus("Loading upgrades list...");
            DefaultUpgrades = await Logic.GenerateUpgradeList();
            NewUpgrades = DefaultUpgrades.Values
                .Select(x=>x.Clone())
                .OrderBy(x => x.DisplayName).ToList();

            await UpdateDisplayNames(NewUpgrades);
            dgvUpgrades.DataSource = NewUpgrades;
        }
        
        private async void Reset_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(Reset);

            await Initialize();

            IoC.SetStatus("Reset complete");
        }

        private void ResetSelected_Click(object sender, EventArgs e)
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
            if (_active)
                ResetSelected.Enabled = dgvUpgrades.SelectedRows.Count > 0;
        }
    }
}
