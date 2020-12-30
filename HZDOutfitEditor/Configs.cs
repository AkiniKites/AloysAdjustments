using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HZDOutfitEditor
{
    public class Configs
    {
        public static string GamePackDir => Path.Combine(IoC.Config.Settings.GamePath, IoC.Config.PackDir);
    }
}
