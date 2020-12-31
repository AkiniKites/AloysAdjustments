using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;

namespace AloysAdjustments.Configuration
{
    public class Configs
    {
        public static string GamePackDir => Path.Combine(IoC.Settings.GamePath, IoC.Config.PackDir);
        public static string PatchPath => Path.Combine(GamePackDir, IoC.Config.PatchFile);
    }
}
