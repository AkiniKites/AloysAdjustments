using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using AloysAdjustments.Logic;
using Newtonsoft.Json;

namespace AloysAdjustments.Utility
{
    public class GameCache<T>
    {
        private class CacheData
        {
            public string Path { get; set; }
            public string Version { get; set; }
            public T Data { get; set; }
        }

        private string CachePath => Path.Combine(IoC.Config.CachePath, $"{Name}.json");

        public string Name { get; }
        
        public GameCache(string name)
        {
            Name = name;
        }

        public bool TryLoadCache(out T data)
        {
            data = default(T);

            if (IoC.Debug.DisableGameCache)
                return false;
            if (!File.Exists(CachePath))
                return false;

            var json = File.ReadAllText(CachePath);
            var cache = JsonConvert.DeserializeObject<CacheData>(
                json, new BaseGGUUIDConverter());

            if (cache == null || cache.Path != IoC.Settings.GamePath)
            {
                ClearCache();
                return false;
            }

            data = cache.Data;
            return true;
        }

        public void Save(T data)
        {
            var cache = new CacheData()
            {
                Path = IoC.Settings.GamePath,
                Version = IoC.CurrentVersion.ToString(),
                Data = data
            };
            var json = JsonConvert.SerializeObject(cache,
                Formatting.Indented, new BaseGGUUIDConverter());

            CheckDirectory();
            File.WriteAllText(CachePath, json);

            IoC.Notif.CacheUpdate?.Invoke();
        }

        public void ClearCache()
        {
            if (File.Exists(CachePath))
                File.Delete(CachePath);

            IoC.Notif.CacheUpdate?.Invoke();
        }

        private void CheckDirectory()
        {
            Paths.CheckDirectory(Path.GetDirectoryName(CachePath));
        }
    }
}
