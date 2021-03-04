﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;

namespace AloysAdjustments.Plugins.NPC.Configuration
{
    public class NpcSettings : IPluginSettings
    {
        public bool ApplyToAll { get; set; }
        public int ModelFilter { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
