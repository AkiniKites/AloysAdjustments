using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.UI;

namespace AloysAdjustments.Modules
{
    public abstract class ModuleBase : UserControl, IModule
    {
        public abstract string ModuleName { get; }

        public ControlRelay Reset { get; set; }
        public ControlRelay ResetSelected { get; set; }

        protected ModuleBase()
        {
            Reset = new ControlRelay();
            ResetSelected = new ControlRelay();

            Reset.Click += Reset_Click;
            ResetSelected.Click += ResetSelected_Click;
        }

        public abstract Task LoadPatch(string path);
        public abstract Task CreatePatch(string patchDir);
        public abstract Task Initialize();

        protected virtual void Reset_Click() { }
        protected virtual void ResetSelected_Click() { }
    }
}
