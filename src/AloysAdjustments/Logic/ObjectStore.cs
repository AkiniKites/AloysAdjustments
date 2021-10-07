using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Common.JsonConverters;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Utility;
using Newtonsoft.Json;

namespace AloysAdjustments.Logic
{
    public static class ObjectStore
    {
        private const string Prefix = "ObjectStore-";

        public static void AddObject<T>(this Patch patch, T obj, string name)
        {
            var json = JsonConvert.SerializeObject(obj, JsonHelper.Converters);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            patch.AddFile(ms, $"{Prefix}{name}");
        }

        public static async Task<T> LoadObjectAsync<T>(string path, string name)
        {
            return await Async.Run(() => LoadObject<T>(path, name));
        }
        public static T LoadObject<T>(string path, string name)
        {
            using var s = IoC.Archiver.LoadFileStream(path, $"{Prefix}{name}");
            if (s == null)
                return default;

            using var reader = new StreamReader(s);
            var json = reader.ReadToEnd();

            try
            {
                return JsonConvert.DeserializeObject<T>(json, JsonHelper.Converters);
            }
            catch (Exception ex)
            {
                return default;
            }
        }
    }
}
