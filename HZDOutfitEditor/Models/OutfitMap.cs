using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HZDOutfitEditor.Models
{
    public class OutfitFile
    {
        public string File { get; set; }
        public List<Outfit> Outfits { get; set; }

        public OutfitFile()
        {
            Outfits = new List<Outfit>();
        }

        public OutfitFile Clone()
        {
            return new OutfitFile()
            {
                File = File,
                Outfits = Outfits.Select(x => x.Clone()).ToList()
            };
        }
    }
}
