using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Plugins.Common.Characters;
using AloysAdjustments.Plugins.Common.Data;
using Decima;
using Decima.HZD;

namespace AloysAdjustments.Plugins.Outfits
{
    public class OutfitCharacterGenerator
    {
        public const string VariantNameFormat = "-AA-";
        public static readonly Regex VariantNameMatcher = new Regex($"^(?<name>.+?){VariantNameFormat}(?<id>.+)$");
        
        private readonly CharacterGenerator _charGen;

        public OutfitCharacterGenerator()
        {
            _charGen = new CharacterGenerator();
        }

        public List<CharacterModel> GetCharacterModels(bool all)
        {
            var chars = _charGen.GetCharacterModels(true);
            if (all)
                chars.AddRange(_charGen.GetCharacterModels(false));
            return chars;
        }

        public async Task<Dictionary<BaseGGUUID, BaseGGUUID>> GetVariantMapping(string path, OutfitsGenerator outfitsLogic)
        {
            var variantMapping = new Dictionary<BaseGGUUID, BaseGGUUID>();

            var models = outfitsLogic.GenerateModelList(f =>
                IoC.Archiver.LoadFile(path, f));
            foreach (var model in models)
            {
                var core = await IoC.Archiver.LoadFileAsync(path, model.Source);
                if (core == null) 
                    continue;

                if (!core.GetTypesById<HumanoidBodyVariant>().TryGetValue(model.Id, out var variant))
                    continue;

                var name = VariantNameMatcher.Match(variant.Name);
                if (name.Success)
                {
                    variantMapping.Add(model.Id, name.Groups["id"].Value);
                }
            }

            return variantMapping;
        }
    }
}
