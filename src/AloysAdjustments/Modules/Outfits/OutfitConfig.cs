using System;
using System.Collections.Generic;
using System.Text;

namespace AloysAdjustments.Modules.Outfits
{
    public class OutfitConfig
    {
        public string AloyCharacterName { get; set; }
        public string PlayerComponentsFile { get; set; }
        public string PlayerCharacterFile { get; set; }
        public string[] OutfitFiles { get; set; }

        public string HumanoidMatcher { get; set; }
        public string UniqueHumanoidMatcher { get; set; }
        public string[] IgnoredCharacters { get; set; }
    }
}
