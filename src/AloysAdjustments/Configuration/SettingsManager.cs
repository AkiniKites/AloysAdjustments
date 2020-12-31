using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Configuration
{
    public class SettingsManager
    {
        private const string SettingsFile = "settings.json";
        private static readonly string ApplicationSettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            typeof(SettingsManager).Assembly.GetName().Name);

        private static string SettingsPath => Path.Combine(ApplicationSettingsPath, SettingsFile);

        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private static UserSettings _settings;

        public static async Task<UserSettings> Load()
        {
            if (_settings != null)
                _settings.PropertyChanged -= Settings_PropertyChanged;

            if (File.Exists(SettingsPath))
            {
                await using var fs = File.OpenRead(SettingsPath);
                _settings = await JsonSerializer.DeserializeAsync<UserSettings>(fs);
            }
            else
            {
                _settings = new UserSettings();
            }

            _settings.PropertyChanged += Settings_PropertyChanged;
            return _settings;
        }

        private static async void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
             await Save(_settings);
        }

        private static async Task Save(UserSettings settings)
        {
            try
            {
                await _lock.WaitAsync();

                Paths.CheckDirectory(ApplicationSettingsPath);

                await using var fs = File.Open(SettingsPath, FileMode.Create);
                await JsonSerializer.SerializeAsync(fs, settings, new JsonSerializerOptions()
                {
                    WriteIndented = true
                });
            }
            finally
            {
                _lock.Release();
            }
        }

    }
}
