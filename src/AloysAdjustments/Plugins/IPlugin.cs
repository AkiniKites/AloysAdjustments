using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;

namespace AloysAdjustments.Plugins
{
    public interface IPlugin
    {
        void ApplyChanges(Patch patch);
    }
}
