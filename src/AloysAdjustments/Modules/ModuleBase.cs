using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.UI;

namespace AloysAdjustments.Modules
{
    //can't be abstract because win forms designer is shit
    public class ModuleBase : UserControl, IModule
    {
        public virtual string ModuleName { get; }

        public ControlRelay Reset { get; set; }
        public ControlRelay ResetSelected { get; set; }

        protected ModuleBase()
        {
            Reset = new ControlRelay();
            ResetSelected = new ControlRelay();

            Reset.Click += Reset_Click;
            ResetSelected.Click += ResetSelected_Click;
        }

        public virtual Task LoadPatch(string path) => null;
        public virtual Task CreatePatch(string patchDir) => null;
        public virtual Task Initialize() => null;
        public virtual bool ValidateChanges() => true;

        protected virtual void Reset_Click() { }
        protected virtual void ResetSelected_Click() { }
    }
}
