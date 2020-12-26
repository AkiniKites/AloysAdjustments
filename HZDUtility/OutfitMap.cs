using Decima;
using System;
using System.Collections.Generic;
using System.Text;

namespace HZDUtility
{
    public class OutfitMap
    {
        public string File { get; set; }
        public List<(BaseGGUUID Ref, BaseGGUUID ArmorId)> Refs { get; set; }

        public OutfitMap()
        {
            Refs = new List<(BaseGGUUID Ref, BaseGGUUID ArmorId)>();
        }
    }
}
