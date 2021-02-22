using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Utility;
using Newtonsoft.Json;

namespace AloysAdjustments.Configuration
{
    public class Configs
    {
        private const string ConfigPath = "config.json";

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

        public static void LoadConfigs()
        {
            var json = File.ReadAllText(ConfigPath);
            IoC.Bind(JsonConvert.DeserializeObject<Config>(json));
            IoC.Bind(SettingsManager.Load());
        }
    }
}
