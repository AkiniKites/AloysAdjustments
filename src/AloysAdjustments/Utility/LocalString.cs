using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;
using Decima.HZD;

namespace AloysAdjustments.Utility
{
    public class LocalString
    {
        public string File { get; }
        public BaseGGUUID Id { get; }

        public LocalString(string file, BaseGGUUID id)
        {
            File = file;
            Id = BaseGGUUID.FromOther(id);
        }

        public LocalString Clone()
        {
            return new LocalString(File, Id);
        }

        public static implicit operator LocalString(Ref<LocalizedTextResource> val)
        {
            if (val == null)
                return null;
            return new LocalString(val.ExternalFile, val.GUID);
        }
    }
}
