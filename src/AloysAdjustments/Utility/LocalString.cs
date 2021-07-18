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
        public string File { get; set; }
        public BaseGGUUID Id { get; set; }

        public LocalString Clone()
        {
            return new LocalString()
            {
                File = File,
                Id = BaseGGUUID.FromOther(Id)
            };
        }

        public static implicit operator LocalString(Ref<LocalizedTextResource> val)
        {
            if (val == null)
                return null;
            return new LocalString()
            {
                File = val.ExternalFile,
                Id = val.GUID
            };
        }
    }
}
