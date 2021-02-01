using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.UI;

namespace AloysAdjustments.Plugins
{
    public interface IPlugin
    {
        void ApplyChanges(Patch patch);
    }
}
