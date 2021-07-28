using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Plugins.Common.Characters;
using AloysAdjustments.Plugins.Common.Data;

namespace AloysAdjustments.Plugins.Outfits.Compatibility
{
    public class AllCharacterGenerator : CharacterGenerator
    {
        public AllCharacterGenerator() : base(false) { }

        protected override IEnumerable<CharacterModel> ConsolidateModels(IEnumerable<CharacterModel> models)
        {
            return models;
        }

        public new int GetModelSorting(CharacterModel model)
        {
            return base.GetModelSorting(model);
        }
    }
}
