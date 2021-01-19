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
    public static class HzdCoreExtensions
    {
        public static Dictionary<BaseGGUUID, T> GetTypesById<T>(
            this HzdCore core, string typeName = null) where T : RTTIRefObject
        {
            typeName ??= typeof(T).Name;

            return core.Binary.Where(x => x.GetType().Name == typeName)
                .ToDictionary(x => (BaseGGUUID)((T)x).ObjectUUID, x => (T)x);
        }
        public static List<T> GetTypes<T>(this HzdCore core, string typeName = null)
        {
            typeName ??= typeof(T).Name;

            return core.Binary.Where(x => x.GetType().Name == typeName)
                .Cast<T>().ToList();
        }
        public static T GetType<T>(this HzdCore core, string typeName = null)
        {
            return GetTypes<T>(core, typeName).FirstOrDefault();
        }
    }
}
