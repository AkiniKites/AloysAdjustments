using System;
using System.Collections.Generic;
using System.Text;

namespace AloysAdjustments.Data
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
