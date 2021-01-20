using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.UI;

namespace AloysAdjustments.Modules
{
    public interface IModule
    {
        string ModuleName { get; }

        ControlRelay Reset { get; }
        ControlRelay ResetSelected { get; }

        Task LoadPatch(string path);
        void ApplyChanges(Patch patch);

        Task Initialize();
        bool ValidateChanges();
    }
}
