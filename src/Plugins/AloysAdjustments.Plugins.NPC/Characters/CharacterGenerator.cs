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
        public const string VariantNameFormat = "-AA-";
        public static readonly Regex VariantNameMatcher = new Regex($"^(?<name>.+?){VariantNameFormat}(?<id>.+)$");

        private readonly Regex HumanoidMatcher;
        private readonly Regex UniqueHumanoidMatcher;
        private readonly string[] Ignored;

        private readonly GameCache<List<CharacterModel>> _cache;
        private readonly object _lock = new object();

        public CharacterGenerator()
        {
            HumanoidMatcher = new Regex(IoC.Get<CharacterConfig>().HumanoidMatcher);
            UniqueHumanoidMatcher = new Regex(IoC.Get<CharacterConfig>().UniqueHumanoidMatcher);
            Ignored = IoC.Get<CharacterConfig>().IgnoredCharacters.ToArray();

            _cache = new GameCache<List<CharacterModel>>("npcs");
        }

        public List<CharacterModel> GetCharacterModels(bool unique)
        {
            lock (_lock)
            {
                return LoadCharacterModels(unique);
            }
        }
        private List<CharacterModel> LoadCharacterModels(bool unique)
        {
            if (_cache.TryLoadCache(out var cached))
            {
                var validCached = cached.Where(x => x.UniqueCharacter == unique).ToList();
                if (validCached.Any())
                    return validCached;
            }
            else
                cached = new List<CharacterModel>();

            var files = Prefetch.Load().Files.Keys;

            var modelBag = new ConcurrentBag<CharacterModel>();

            var tasks = new ParallelTasks<string>(
                Environment.ProcessorCount, file =>
                {
                    if (IsValid(file, unique))
                    {
                        foreach (var model in GetCharacterModels(file))
                            modelBag.Add(model);
                    }
                });

            tasks.Start();
            tasks.AddItems(files);
            tasks.WaitForComplete();

            GC.Collect();

            var modelList = modelBag.ToList();
            cached.AddRange(modelList);
            _cache.Save(cached);

            return modelList;
        }

        private bool IsValid(string file, bool unique)
        {
            if (!IsHumanoid(file))
                return false;
            if (IsUnique(file) != unique)
                return false;

            for (int i = 0; i < Ignored.Length; i++)
            {
                if (file.Contains(Ignored[i]))
                    return false;
            }

            return true;
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
