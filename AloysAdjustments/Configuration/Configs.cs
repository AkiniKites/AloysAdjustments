using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments
{
    public class Configs
    {
        public static string GamePackDir => Path.Combine(IoC.Settings.GamePath, IoC.Config.PackDir);
    }
}
