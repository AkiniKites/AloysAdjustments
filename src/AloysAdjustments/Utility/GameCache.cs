using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AloysAdjustments.Logic;
using Newtonsoft.Json;

namespace AloysAdjustments.Utility
{
    public class GameCache<T>
    {
        private string CachePath => Path.Combine(IoC.Config.CachePath, $"{Name}.json");

        public string Name { get; }
        
        public GameCache(string name)
        {
            Name = name;
        }

        public bool TryLoadCache(out T data)
        {
            data = default(T);

            if (!File.Exists(CachePath))
                return false;

            var json = File.ReadAllText(CachePath);
            var cache = JsonConvert.DeserializeObject<(string Path, T Data)>(
                json, new BaseGGUUIDConverter());

            if (cache.Path != IoC.Settings.GamePath)
            {
                ClearCache();
                return false;
            }

            data = cache.Data;
            return true;
        }

        public void Save(T data)
        {
            var cache = (Path: IoC.Settings.GamePath, Data: data);
            var json = JsonConvert.SerializeObject(cache,
                Formatting.Indented, new BaseGGUUIDConverter());

            CheckDirectory();
            File.WriteAllText(CachePath, json);
        }

        public void ClearCache()
        {
            if (File.Exists(CachePath))
                File.Delete(CachePath);
        }

        private void CheckDirectory()
        {
            Paths.CheckDirectory(Path.GetDirectoryName(CachePath));
        }
    }
}
