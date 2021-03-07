using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;

namespace AloysAdjustments.Plugins.CustomFiles.Configuration
{
    public class ModSettings : IPluginSettings
    {
        public List<Mod> Mods { get; set; }

        public ModSettings()
        {
            Mods = new List<Mod>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
