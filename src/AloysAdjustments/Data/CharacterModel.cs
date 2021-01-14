using System;
using System.Collections.Generic;
using System.Text;
using Decima;

namespace AloysAdjustments.Data
{
    public class CharacterModel : Model
    {
        public override string ToString()
        {
            return "Character - " + Name;
        }
    }
}
