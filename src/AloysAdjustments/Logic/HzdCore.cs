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
        public string Source { get; set; }
        public string FilePath { get; private set; }
        public List<object> Components { get; private set; }

        public static HzdCore Load(string file, string source)
        {
            try
            {
                return new HzdCore()
                {
                    FilePath = file,
                    Source = source,
                    Components = CoreBinary.Load(file, true)
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
                return new HzdCore()
                {
                    Source = source,
                    Components = CoreBinary.Load(stream, true)
                };
            }
            catch (Exception ex)
            {
                throw new HzdException($"Failed to load: {source ?? "null"}", ex);
            }
        }

        private HzdCore() { }

        public async Task Save(string filePath = null)
        {
            await Async.Run(() =>
            {
                var savePath = filePath ?? FilePath;
                if (savePath == null)
                    throw new HzdException("Cannot save pack file, save path null");

                CoreBinary.Save(savePath, Components);
            });
        }

        public Dictionary<BaseGGUUID, T> GetTypesById<T>(string typeName = null) where T : RTTIRefObject
        {
            typeName ??= typeof(T).Name;

            return Components.Where(x => x.GetType().Name == typeName)
                .ToDictionary(x => (BaseGGUUID)((T)x).ObjectUUID, x => (T)x);
        }
        public List<T> GetTypes<T>(string typeName = null)
        {
            typeName ??= typeof(T).Name;

            return Components.Where(x => x.GetType().Name == typeName)
                .Cast<T>().ToList();
        }
    }
}
