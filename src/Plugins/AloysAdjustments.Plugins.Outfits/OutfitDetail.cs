using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Plugins.Common.Data;
using AloysAdjustments.Plugins.Outfits.Data;
using Decima;

namespace AloysAdjustments.Plugins.Outfits
{
    public class OutfitDetail
    {
        public Outfit Outfit { get; set; }
        public Model Model { get; set; }
        public BaseGGUUID VariantId { get; set; }

        public Model DefaultModel { get; set; }

        public bool Modified => Outfit.Modified;
        public bool IsCharacter => Model is CharacterModel;
    }
}