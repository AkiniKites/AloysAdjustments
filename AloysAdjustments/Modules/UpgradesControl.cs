using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Modules
{
    public partial class UpgradesControl : UserControl, IModule
    {
        private UpgradesLogic Logic { get; set; }

        public string ModuleName => "Inventory";
        public Control ModuleControl => this;

        public Button Reset { get; set; }
        public Button ResetSelected { get; set; }

        public UpgradesControl()
        {
            Logic = new UpgradesLogic();

            InitializeComponent();
        }
        
        public void Activate()
        {
            Reset.Click += Reset_Click;
            ResetSelected.Click += ResetSelected_Click;

            //ResetSelected.Enabled = lbOutfits.SelectedIndex >= 0;
        }

        public void DeActivate()
        {
            Reset.Click -= Reset_Click;
            ResetSelected.Click -= ResetSelected_Click;
        }

        public Task Load(string path)
        {
            return null;
        }

        public Task CreatePatch(string patchDir)
        {
            return null;
        }

        public Task Initialize()
        {
            return null;
        }
        
        private async void Reset_Click(object? sender, EventArgs e)
        {
            using var _ = new ControlLock(Reset);

            await Initialize();

            IoC.SetStatus("Reset complete");
        }

        private void ResetSelected_Click(object? sender, EventArgs e)
        {
        }
    }
}
