using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Logic;
using AloysAdjustments.UI;

namespace AloysAdjustments.Modules
{
    public interface IModule
    {
        string ModuleName { get; }

        ControlRelay Reset { get; }
        ControlRelay ResetSelected { get; }

        Task LoadPatch(string path);
        Task ApplyChanges(Patch patch);

        Task Initialize();
        bool ValidateChanges();
    }
}
