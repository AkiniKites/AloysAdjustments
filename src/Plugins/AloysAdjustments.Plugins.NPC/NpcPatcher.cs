using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.Common.Data;
using AloysAdjustments.Plugins.NPC.Characters;
using Decima;
using Decima.HZD;
using Model = AloysAdjustments.Plugins.Common.Data.Model;

namespace AloysAdjustments.Plugins.NPC
{
    public class NpcPatcher
    {
        public void CreatePatch(Patch patch, IList<ValuePair<Model>> npcs)
        {
            var modifiedNpcs = npcs.Where(x => x.Modified).ToList();

            if (!modifiedNpcs.Any())
                return;
            
            var npcMap = new CharacterReferences().LoadMap(modifiedNpcs.Select(x => x.Default));

            foreach (var npc in modifiedNpcs)
            {
                if (!npcMap.TryGetValue(npc.Default, out var files))
                    continue;

                foreach (var file in files)
                {
                    UpdateCharacterReference(patch, npc, file);
                }
            }
        }

        public void UpdateCharacterReference(Patch patch, ValuePair<Model> npc, string file)
        {
            var core = patch.AddFile(file);

            var refs = new List<BaseRef>();
            core.Binary.VisitAllObjects<BaseRef>((refId, parent) =>
            {
                if (!(parent is HumanoidBodyVariant) && refId.GUID == npc.Default.Id)
                    refs.Add(refId);
            });

            if (!refs.Any())
                return;

            foreach (var refId in refs)
            {
                refId.Type = BaseRef.Types.ExternalCoreUUID;
                refId.ExternalFile = npc.Value.Source;
                refId.GUID = npc.Value.Id;
            }

            core.Save();
        }
    }
}
