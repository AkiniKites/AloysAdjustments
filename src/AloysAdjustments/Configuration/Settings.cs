using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Plugins;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Configuration
{
    public interface IPluginSettings : INotifyPropertyChanged
    {
    }

    public static class Settings
    {
        public static void BindModuleSettings<T>(string name) where T : IPluginSettings, new()
        {
            var settings = IoC.Settings.PluginSettings.TryGetValue(name, out var config) ? config.ToObject<T>() : new T();
            SettingsManager.AddPlugin(name, settings);
            IoC.Bind(settings);
        }

        public static void Save()
        {
            SettingsManager.Save();
        }
    }
}
