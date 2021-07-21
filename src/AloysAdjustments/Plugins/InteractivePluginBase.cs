using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.UI;

namespace AloysAdjustments.Plugins
{
    public abstract class InteractivePluginControl : UserControl, IInteractivePlugin
    {
        public abstract string PluginName { get; }
        public Control PluginControl => this;

        public ControlRelay Reset { get; set; }
        public ControlRelay ResetSelected { get; set; }

        protected InteractivePluginControl()
        {
            Reset = new ControlRelay();
            ResetSelected = new ControlRelay();

            Reset.Click += Reset_ClickCommand;
            ResetSelected.Click += ResetSelected_ClickCommand;
        }

        public abstract Task LoadPatch(string path);
        public abstract void ApplyChanges(Patch patch);
        public abstract Task Initialize();
        public virtual bool ValidateChanges() => true;

        private async void Reset_ClickCommand() => await Relay.To(Reset_Click);
        protected virtual Task Reset_Click() => Task.CompletedTask;

        private async void ResetSelected_ClickCommand() => await Relay.To(ResetSelected_Click);
        protected virtual Task ResetSelected_Click() => Task.CompletedTask;

        public virtual Task CommandAction(string command) => Task.CompletedTask;
    }
}
