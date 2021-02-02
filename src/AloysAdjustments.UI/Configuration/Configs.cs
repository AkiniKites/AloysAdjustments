using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Configuration
{
    public class Configs
    {
        public static string GamePackDir => 
            IoC.Settings.GamePath == null ? null : Path.Combine(IoC.Settings.GamePath, IoC.Config.PackDir);
        public static string PatchPath => 
            GamePackDir == null ? null : Path.Combine(GamePackDir, IoC.Config.PatchFile);

        public static T LoadModuleConfig<T>(string name)
        {
            if (!IoC.Config.ModuleConfigs.TryGetValue(name, out var config))
                throw new HzdException($"No module config found for: {name}");

            return config.ToObject<T>();
        }
    }
}
