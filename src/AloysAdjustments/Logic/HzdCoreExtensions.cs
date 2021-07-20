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
        public static Dictionary<BaseGGUUID, RTTIRefObject> GetTypesById(this HzdCore core)
            => GetTypesById<RTTIRefObject>(core);

        public static Dictionary<BaseGGUUID, T> GetTypesById<T>(
            this HzdCore core) where T : RTTIRefObject
        {
            return core.Binary.Where(x => x is T)
                .ToDictionary(x => (BaseGGUUID)((T)x).ObjectUUID, x => (T)x);
        }

        public static List<T> GetTypes<T>(this HzdCore core)
        {
            return core.Binary.Where(x => x is T).Cast<T>().ToList();
        }

        public static T GetType<T>(this HzdCore core)
        {
            return GetTypes<T>(core).FirstOrDefault();
        }

        public static RTTIRefObject GetTypeById(this HzdCore core, BaseGGUUID id)
        {
            foreach (var obj in core.Binary)
            {
                if (obj is RTTIRefObject refObj && refObj.ObjectUUID.Equals(id))
                    return refObj;
            }

            return null;
        }
    }
}
