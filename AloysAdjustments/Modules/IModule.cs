using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AloysAdjustments.Modules
{
    public interface IModule
    {
        string ModuleName { get; }
        Control ModuleControl { get; }

        Button Reset { get; set; }
        Button ResetSelected { get; set; }

        void Activate();
        void DeActivate();
        Task Load(string path);
        Task CreatePatch(string patchDir);

        Task Initialize();
    }
}
