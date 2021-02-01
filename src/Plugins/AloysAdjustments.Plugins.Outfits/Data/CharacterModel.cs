using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.Outfits.Data
{
    public class CharacterModel : Model
    {
        public override string ToString()
        {
            return "Character - " + Name;
        }
    }
}
