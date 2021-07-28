using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic.Compatibility;
using Decima;

namespace AloysAdjustments.Plugins.Outfits.Compatibility
{
    public class OutfitCompatibility
    {
        private OutfitCharacterGenerator _outfitCharGen;
        private AllCharacterGenerator _allCharGen;

        public async Task<Dictionary<BaseGGUUID, BaseGGUUID>> UpdateVariants(
            string path, Dictionary<BaseGGUUID, BaseGGUUID> mapping)
        {
            //return mapping;
            if (!FileCompatibility.ShouldMigrate(path, new Version(1, 7, 5)))
                return mapping;

            if (_outfitCharGen == null)
            {
                _allCharGen = new AllCharacterGenerator();
                _outfitCharGen = new OutfitCharacterGenerator(_allCharGen);
            }

            var allModels = await Task.Run(() => _outfitCharGen.GetCharacterModels(true));

            var modelRemapping = new Dictionary<BaseGGUUID, BaseGGUUID>();
            foreach (var modelGroup in allModels.GroupBy(x => x.Name))
            {
                var newModel = modelGroup.OrderBy(_allCharGen.GetModelSorting).First();
                foreach (var model in modelGroup)
                    modelRemapping.Add(model.Id, newModel.Id);
            }

            var newMapping = new Dictionary<BaseGGUUID, BaseGGUUID>();
            foreach (var map in mapping)
            {
                if (!modelRemapping.TryGetValue(map.Value, out var id))
                    id = map.Value;
                
                newMapping.Add(map.Key, id);
            }

            return newMapping;
        }
    }
}
