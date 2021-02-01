using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.Misc.Data
{
    public class MiscAdjustments
    {
        public bool? SkipIntroLogos { get; set; }

        public MiscAdjustments Clone()
        {
            return new MiscAdjustments()
            {
                SkipIntroLogos = SkipIntroLogos
            };
        }
    }
}
