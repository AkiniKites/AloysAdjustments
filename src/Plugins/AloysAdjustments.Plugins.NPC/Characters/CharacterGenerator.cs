using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.Common;
using AloysAdjustments.Plugins.Common.Data;
using AloysAdjustments.Utility;
using Decima.HZD;
using HZDCoreEditor.Util;

namespace AloysAdjustments.Plugins.NPC.Characters
{
    public class CharacterGenerator
    {
        private readonly Regex HumanoidMatcher;
        private readonly Regex UniqueHumanoidMatcher;

        private readonly FileCollector<CharacterModel> _uniqueFileCollector;
        private readonly FileCollector<CharacterModel> _normalFileCollector;

        public CharacterGenerator()
        {
            HumanoidMatcher = new Regex(IoC.Get<CharacterConfig>().HumanoidMatcher);
            UniqueHumanoidMatcher = new Regex(IoC.Get<CharacterConfig>().UniqueHumanoidMatcher);
            var ignored = IoC.Get<CharacterConfig>().IgnoredFiles;

            _uniqueFileCollector = new FileCollector<CharacterModel>("npc-u",
                f => IsHumanoid(f) && IsUnique(f), GetCharacterModels, ignored);
            _normalFileCollector = new FileCollector<CharacterModel>("npc-n",
                f => IsHumanoid(f) && !IsUnique(f), GetCharacterModels, ignored);
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

        private List<CharacterModel> GetCharacterModels(string file)
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
    }
}
