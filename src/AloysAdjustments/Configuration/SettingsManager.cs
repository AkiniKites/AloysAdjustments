using AloysAdjustments.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using Newtonsoft.Json.Linq;

namespace AloysAdjustments.Configuration
{
    internal class SettingsManager
    {
        private const string SettingsFile = "settings.json";
        private static readonly string ApplicationSettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Aloy's Adjustments");

        private static string SettingsPath => Path.Combine(ApplicationSettingsPath, SettingsFile);

        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private static UserSettings _settings;
        private static Dictionary<string, IPluginSettings> _pluginSettings = new Dictionary<string, IPluginSettings>();

        public static UserSettings Load()
        {
            if (_settings != null)
                _settings.PropertyChanged -= Settings_PropertyChanged;

            var changed = false;

            if (!FileBackup.RunWithBackup(SettingsPath, () =>
            {
                var json = File.ReadAllText(SettingsPath);
                if (String.IsNullOrEmpty(json))
                    return false;

                var obj = JObject.Parse(json);
                changed = Compatibility.MigrateSettings(obj);
                _settings = obj.ToObject<UserSettings>();
                
                return true;
            }))
            {
                _settings = new UserSettings();
            }

            if (changed)
                Save(_settings);

            _settings.PropertyChanged += Settings_PropertyChanged;
            return _settings;
        }

        public static void Save()
        {
            Save(_settings);
        }

        public static void AddPlugin(string name, IPluginSettings settings)
        {
            _pluginSettings.Add(name, settings);
            settings.PropertyChanged += Settings_PropertyChanged;
        }

        private static async void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
             await SaveAsync(_settings);
        }

        private static async Task SaveAsync(UserSettings settings)
        {
            await Async.Run(() => Save(settings));
        }
        private static void Save(UserSettings settings)
        {
            try
            {
                _lock.Wait();

                Paths.CheckDirectory(ApplicationSettingsPath);

                using var backup = new FileBackup(SettingsPath);

                settings.Version = Compatibility.CurrentSettingsVersion;

                //clear and assign to prevent property event
                settings.PluginSettings.Clear();
                foreach (var plugin in _pluginSettings)
                {
                    settings.PluginSettings.Add(plugin.Key, JObject.FromObject(plugin.Value));
                }

                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsPath, json);

                backup.Delete();
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
