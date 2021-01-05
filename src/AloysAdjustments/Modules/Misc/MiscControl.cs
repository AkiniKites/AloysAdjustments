using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Modules.Misc
{
    public class MiscControl : ModuleBase
    {
        public override string ModuleName => "Misc";

        public override Task LoadPatch(string path) => null;
        public override Task CreatePatch(string patchDir) => null;
        public override Task Initialize() => null;
        public override bool ValidateChanges() => true;

        protected override void Reset_Click() { }
        protected override void ResetSelected_Click() { }
    }
}
