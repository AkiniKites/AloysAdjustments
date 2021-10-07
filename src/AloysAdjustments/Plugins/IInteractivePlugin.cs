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
    public interface IInteractivePlugin
    {
        string PluginName { get; }
        Control PluginControl { get; }

        ControlRelay Reset { get; }
        ControlRelay ResetSelected { get; }

        Task LoadPatch(string path);
        void ApplyChanges(Patch patch);

        Task Initialize();
        bool ValidateChanges();

        Task CommandAction(string command);
    }
}
