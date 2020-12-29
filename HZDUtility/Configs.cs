using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HZDUtility
{
    public class Configs
    {
        public static string GamePackDir => Path.Combine(IoC.Config.Settings.GamePath, IoC.Config.PackDir);
    }
}
