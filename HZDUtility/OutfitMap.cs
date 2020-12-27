using Decima;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZDUtility
{
    public class OutfitMap
    {
        public string File { get; set; }
        public List<(BaseGGUUID Model, BaseGGUUID RefId)> Refs { get; set; }

        public OutfitMap()
        {
            Refs = new List<(BaseGGUUID Model, BaseGGUUID RefId)>();
        }

        public OutfitMap Clone()
        {
            var map = new OutfitMap()
            {
                File = File
            };

            map.Refs = Refs
                .Select(x => (BaseGGUUID.FromOther(x.Model), BaseGGUUID.FromOther(x.RefId)))
                .ToList();

            return map;
        }
    }
}
