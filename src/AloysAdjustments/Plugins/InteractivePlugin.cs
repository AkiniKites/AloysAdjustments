using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.UI;

namespace AloysAdjustments.Plugins.NPC
{
    public abstract class InteractivePlugin: IInteractivePlugin
    {
        public abstract string PluginName { get; }
        public Control PluginControl { get; protected set; }
        public ControlRelay Reset { get; protected set; }
        public ControlRelay ResetSelected { get; protected set; }

        public abstract Task LoadPatch(string path);
        public abstract void ApplyChanges(Patch patch);
        public abstract Task Initialize();

        public virtual bool ValidateChanges() => true;
    }
}
