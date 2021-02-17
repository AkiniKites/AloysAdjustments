using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.Outfits.Data;
using AloysAdjustments.Utility;
using HZDCoreEditor.Util;
using Newtonsoft.Json;

namespace AloysAdjustments.Plugins.NPC.Characters
{
    public class CharacterReferences
    {
        private const string ReferenceMap = "data\\npc-map.json";
        
        public Dictionary<Model, string[]> LoadMap(IEnumerable<Model> models)
        {
            if (!File.Exists(ReferenceMap))
                throw new HzdException("Unable to find npc reference map: " + ReferenceMap);
            var json = File.ReadAllText(ReferenceMap);
            var npcRefs = JsonConvert.DeserializeObject<List<CharacterReference>>(json);

            var map = new Dictionary<Model, string[]>();
            foreach (var model in models)
            {
                var idx = npcRefs.FindIndex(x => x.Source == model.Source && x.Name == model.Name);
                if (idx >= 0)
                {
                    var match = npcRefs[idx];
                    npcRefs.RemoveAt(idx);

                    map.Add(model, match.Files);
                }
                else
                {
                    map.Add(model, new string[0]);
                }
            }

            return map;
        }
    }
}
