using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.Outfits.Data
{
    public class CharacterModel : Model
    {
        public bool UniqueCharacter { get; set; }
        
        public override string ToString()
        {
            var name = Name.Replace("DLC1", "DLC ");
            return "Character - " + name;
        }
    }
}
