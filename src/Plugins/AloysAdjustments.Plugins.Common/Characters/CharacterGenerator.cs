using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Plugins.Common.Data;
using AloysAdjustments.Utility;
using Decima.HZD;

namespace AloysAdjustments.Plugins.Common.Characters
{
    public class CharacterGenerator
    {
        private readonly Regex HumanoidMatcher;
        private readonly Regex UniqueHumanoidMatcher;

        private readonly FileCollector<CharacterModel> _uniqueFileCollector;
        private readonly FileCollector<CharacterModel> _normalFileCollector;

        public CharacterGenerator(bool useCache = true)
        {
            HumanoidMatcher = new Regex(IoC.Get<CharacterConfig>().HumanoidMatcher);
            UniqueHumanoidMatcher = new Regex(IoC.Get<CharacterConfig>().UniqueHumanoidMatcher);
            var ignored = IoC.Get<CharacterConfig>().IgnoredFiles;

            _uniqueFileCollector = new FileCollector<CharacterModel>("characters-u",
                f => IsHumanoid(f) && IsUnique(f), GetModels)
                .WithIgnored(ignored).WithConsolidate(ConsolidateModels)
                .DisableCaching(!useCache).Build();

            _normalFileCollector = new FileCollector<CharacterModel>("characters-n",
                f => IsHumanoid(f) && !IsUnique(f), GetModels)
                .WithIgnored(ignored).WithConsolidate(ConsolidateModels)
                .DisableCaching(!useCache).Build();
        }

        public List<CharacterModel> GetCharacterModels(bool unique)
        {
            return unique ? _uniqueFileCollector.Load() : _normalFileCollector.Load();
        }
        
        private bool IsUnique(string file)
        {
            return UniqueHumanoidMatcher.IsMatch(file);
        }
        private bool IsHumanoid(string file)
        {
            return HumanoidMatcher.IsMatch(file);
        }

        protected virtual List<CharacterModel> GetModels(string file)
        {
            var pack = IoC.Archiver.LoadGameFile(file);
            var variants = pack.GetTypes<HumanoidBodyVariant>();
            if (!variants.Any())
                return new List<CharacterModel>();

            var unique = IsUnique(file);

            var models = new List<CharacterModel>();
            foreach (var variant in variants)
            {
                models.Add(new CharacterModel()
                {
                    Id = variant.ObjectUUID,
                    Name = variant.Name,
                    Source = pack.Source,
                    UniqueCharacter = unique
                });
            }

            return models;
        }

        protected virtual IEnumerable<CharacterModel> ConsolidateModels(IEnumerable<CharacterModel> models)
        {
            //models with the same name have the same mesh, just different properties on the HumanoidBodyVariant
            //try and take the dlc models
            foreach (var modelGroup in models.GroupBy(x => x.Name))
            {
                var model = modelGroup.OrderBy(GetModelSorting).FirstOrDefault();
                yield return model;
            }
        }

        protected int GetModelSorting(CharacterModel model)
        {
            return model.Source.Contains("dlc1") ? 1 : 0;
        }
    }
}
