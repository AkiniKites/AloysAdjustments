using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;

namespace AloysAdjustments.Logic
{
    public class HzdCore
    {
        public const string CoreExt = ".core";

        public string Source { get; set; }
        public string FilePath { get; private set; }
        public CoreBinary Binary { get; private set; }

        public static HzdCore Load(string file, string source)
        {
            try
            {
                return new HzdCore()
                {
                    FilePath = file,
                    Source = NormalizeSource(source),
                    Binary = CoreBinary.FromFile(file, true)
                };
            }
            catch(Exception ex)
            {
                throw new HzdException($"Failed to load: {source ?? "null"}", ex);
            }
        }
        public static HzdCore Load(Stream stream, string source)
        {
            try
            {
                using var br = new BinaryReader(stream, Encoding.UTF8, true);

                return new HzdCore()
                {
                    Source = NormalizeSource(source),
                    Binary = CoreBinary.FromData(br, true)
                };
            }
            catch (Exception ex)
            {
                throw new HzdException($"Failed to load: {source ?? "null"}", ex);
            }
        }

        private HzdCore() { }

        public async Task SaveAsync(string filePath = null)
        {
            await Async.Run(() => Save(filePath));
        }
        public void Save(string filePath = null)
        {
            var savePath = filePath ?? FilePath;
            if (savePath == null)
                throw new HzdException("Cannot save pack file, save path null");

            savePath = EnsureExt(savePath);

            Binary.ToFile(savePath);
        }

        public Dictionary<BaseGGUUID, T> GetTypesById<T>(string typeName = null) where T : RTTIRefObject
        {
            typeName ??= typeof(T).Name;

            return Binary.Where(x => x.GetType().Name == typeName)
                .ToDictionary(x => (BaseGGUUID)((T)x).ObjectUUID, x => (T)x);
        }
        public List<T> GetTypes<T>(string typeName = null)
        {
            typeName ??= typeof(T).Name;

            return Binary.Where(x => x.GetType().Name == typeName)
                .Cast<T>().ToList();
        }
        public T GetType<T>(string typeName = null)
        {
            return GetTypes<T>().FirstOrDefault();
        }

        private static string NormalizeSource(string path)
        {
            if (Path.GetExtension(path) == CoreExt)
                path = path.Substring(0, path.Length - CoreExt.Length);
            return path.Replace("\\", "/");
        }

        public static string EnsureExt(string path)
        {
            if (!path.EndsWith(HzdCore.CoreExt, StringComparison.OrdinalIgnoreCase))
                path += HzdCore.CoreExt;
            return path;
        }
    }
}
