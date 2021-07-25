using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using AloysAdjustments.Common.Utility;
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

        private static readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _cacheLocks 
            = new ConcurrentDictionary<string, ReaderWriterLockSlim>();

        private string CachePath => Path.Combine(IoC.Config.CachePath, $"{Name}.json");

        public string Name { get; }
        
        public GameCache(string name)
        {
            Name = name;
        }

        public bool TryLoadCache(out T data)
        {
            data = default(T);

            if (IoC.CmdOptions.DisableGameCache)
                return false;

            var cacheLock = _cacheLocks.GetOrAdd(CachePath, x => new ReaderWriterLockSlim());
            string json;

            using (cacheLock.UsingReaderLock())
            {
                if (!File.Exists(CachePath))
                    return false;

                json = File.ReadAllText(CachePath);
            }

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

            var cacheLock = _cacheLocks.GetOrAdd(CachePath, x => new ReaderWriterLockSlim());
            using (cacheLock.UsingWriterLock())
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
