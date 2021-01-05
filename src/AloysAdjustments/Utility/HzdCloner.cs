using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Decima;

namespace AloysAdjustments.Utility
{
    public class HzdCloner
    {
        public static T Clone<T>(T obj)
        {
            if (obj == null) return default(T);

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            using var br = new BinaryReader(ms);

            RTTI.SerializeType(bw, obj);
            ms.Position = 0;
            return RTTI.DeserializeType<T>(br);
        }
    }
}
