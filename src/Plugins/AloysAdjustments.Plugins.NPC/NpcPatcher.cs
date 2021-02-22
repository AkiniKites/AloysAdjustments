using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.Common.Data;
using AloysAdjustments.Plugins.NPC.Characters;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;
using Model = AloysAdjustments.Plugins.Common.Data.Model;

namespace AloysAdjustments.Plugins.NPC
{
    public class NpcPatcher
    {
        private const string MapName = "NpcMap";

        public void CreatePatch(Patch patch, IList<ValuePair<Model>> npcs)
        {
            var modifiedNpcs = npcs.Where(x => x.Modified).ToList();

            if (!modifiedNpcs.Any())
                return;
            
            var npcMap = new CharacterReferences()
                .LoadMap(modifiedNpcs.Select(x => x.Default));

            var modifications = new List<(ValuePair<Model> Npc, string File)>();
            foreach (var npc in modifiedNpcs)
            {
                if (!npcMap.TryGetValue(npc.Default, out var files))
                    continue;
                foreach (var file in files)
                    modifications.Add((npc, file));
            }

            foreach (var mods in modifications.GroupBy(x => x.File))
            {
                UpdateCharacterReference(patch, mods.Key, mods.Select(x => x.Npc));
            }

            var mapping = modifiedNpcs.Select(x => (x.Default.Id, x.Value.Id));
            patch.AddObject(mapping, MapName);
        }

        public void UpdateCharacterReference(Patch patch, string file, IEnumerable<ValuePair<Model>> npcs)
        {
            var core = patch.AddGameFile(file);

            var npcById = npcs.ToSoftDictionary(x => x.Default.Id, x => x.Value);

            var refs = new List<BaseRef>();
            core.Binary.VisitAllObjects<BaseRef>((refId, parent) =>
            {
                if (!(parent is HumanoidBodyVariant) && refId.GUID != null && npcById.ContainsKey(refId.GUID))
                    refs.Add(refId);
            });

            if (!refs.Any())
                return;

            foreach (var refId in refs)
            {
                var npc = npcById[refId.GUID];

                refId.Type = BaseRef.Types.ExternalCoreUUID;
                refId.ExternalFile = npc.Source;
                refId.GUID = npc.Id;
            }

            core.Save();
        }

        public async Task<Dictionary<BaseGGUUID, BaseGGUUID>> LoadMap(string path)
        {
            var map = await ObjectStore.LoadObjectAsync<List<(BaseGGUUID, BaseGGUUID)>>(path, MapName);
            if (map == null)
                return new Dictionary<BaseGGUUID, BaseGGUUID>();

            return map.ToDictionary(x => x.Item1, x => x.Item2);
        }
    }
}
