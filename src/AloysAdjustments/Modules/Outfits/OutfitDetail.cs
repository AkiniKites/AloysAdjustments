using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Data;
using Decima;

namespace AloysAdjustments.Modules.Outfits
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