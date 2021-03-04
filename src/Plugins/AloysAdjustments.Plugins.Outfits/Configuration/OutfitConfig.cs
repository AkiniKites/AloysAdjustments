using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.Outfits.Configuration
{
    public class OutfitConfig
    {
        public string AloyCharacterName { get; set; }
        public string PlayerComponentsFile { get; set; }
        public string PlayerCharacterFile { get; set; }
        public string RagdollComponentsFile { get; set; }
        public string[] OutfitFiles { get; set; }
        public string[] IgnoredOutfits { get; set; }

        public string HumanoidMatcher { get; set; }
        public string UniqueHumanoidMatcher { get; set; }
        public string[] IgnoredCharacters { get; set; }

        public string OutfitFactFile { get; set; }

        public string[] ArmorComponentFiles { get; set; }
        public string[] IgnoredArmorComponents { get; set; }
        
        public string UndergarmentModelFile { get; set; }
        public string Mission4UndergarmentFixFile { get; set; }
    }
}
