using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Decima;
using Decima.HZD;

namespace AloysAdjustments.Utility
{
    public class HzdUtils
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

        public static bool EqualsIgnoreId(RTTIRefObject a, RTTIRefObject b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;

            var oldIdA = a.ObjectUUID;
            var oldIdB = b.ObjectUUID;

            try
            {
                a.ObjectUUID = Guid.Empty;
                b.ObjectUUID = Guid.Empty;

                using var msA = new MemoryStream();
                using var bwA = new BinaryWriter(msA);
                RTTI.SerializeType(bwA, a);

                using var msB = new MemoryStream();
                using var bwB = new BinaryWriter(msB);
                RTTI.SerializeType(bwB, b);

                msA.TryGetBuffer(out var bytesA);
                msB.TryGetBuffer(out var bytesB);

                return ((ReadOnlySpan<byte>)bytesA).SequenceEqual((ReadOnlySpan<byte>)bytesB);
            }
            finally
            {
                a.ObjectUUID = oldIdA;
                b.ObjectUUID = oldIdB;
            }
        }
    }
}
