using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.NPC.Characters
{
    public class CharacterReference
    {
        public string Source { get; set; }
        public string Name { get; set; }
        public string[] Files { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Files?.Length ?? 0})";
        }
    }
}
