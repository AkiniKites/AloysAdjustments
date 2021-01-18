using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Data;
using AloysAdjustments.Logic;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;
using HZDCoreEditor.Util;

namespace AloysAdjustments.Modules.Outfits
{
    public class CharacterGenerator
    {
        public const string VariantNameFormat = "-AA-";
        public static readonly Regex VariantNameMatcher = new Regex($"^(?<name>.+?){VariantNameFormat}(?<id>.+)$");
        
        private readonly Regex HumanoidMatcher;
        private readonly Regex UniqueHumanoidMatcher;
        private readonly string[] Ignored;

        private readonly GameCache<(bool All, List<CharacterModel> Models)> _cache;
        private readonly object _lock = new object();

        public CharacterGenerator()
        {
            HumanoidMatcher = new Regex(IoC.Get<OutfitConfig>().HumanoidMatcher);
            UniqueHumanoidMatcher = new Regex(IoC.Get<OutfitConfig>().UniqueHumanoidMatcher);
            Ignored = IoC.Get<OutfitConfig>().IgnoredCharacters.ToArray();

            _cache = new GameCache<(bool All, List<CharacterModel> Models)>("characters");
        }

        public List<CharacterModel> GetCharacterModels(bool all)
        {
            lock (_lock)
            {
                return LoadCharacterModels(all);
            }
        }
        private List<CharacterModel> LoadCharacterModels(bool all)
        {
            if (_cache.TryLoadCache(out var cached))
            {
                if (cached.All || !all)
                {
                    var validCached = cached.Models.Where(x => IsValid(x.Source, all)).ToList();
                    if (validCached.Any())
                        return validCached;
                }
            }

            var files = Prefetch.Load().Files.Keys;

            var modelBag = new ConcurrentBag<CharacterModel>();

            var tasks = new ParallelTasks<string>(
                Environment.ProcessorCount, file =>
                {
                    if (IsValid(file, all))
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
            _cache.Save((all, modelList));

            return modelList;
        }

        private bool IsValid(string file, bool all)
        {
            var matcher = all ? HumanoidMatcher : UniqueHumanoidMatcher;
            if (!matcher.IsMatch(file))
                return false;
            for (int i = 0; i < Ignored.Length; i++)
            {
                if (file.Contains(Ignored[i]))
                    return false;
            }

            return true;
        }

        private List<CharacterModel> GetCharacterModels(string file)
        {
            var pack = IoC.Archiver.LoadGameFile(file);
            var variants = pack.GetTypes<HumanoidBodyVariant>();
            if (!variants.Any())
                return new List<CharacterModel>();

            var models = new List<CharacterModel>();
            foreach (var variant in variants)
            {
                models.Add(new CharacterModel()
                {
                    Id = variant.ObjectUUID,
                    Name = variant.Name,
                    Source = pack.Source
                }); ;
            }

            return models;
        }

        public Dictionary<BaseGGUUID, BaseGGUUID> GetVariantMapping(string path, OutfitsGenerator outfitsLogic)
        {
            var variantMapping = new Dictionary<BaseGGUUID, BaseGGUUID>();

            var models = outfitsLogic.GenerateModelList(path);
            foreach (var model in models)
            {
                var core = IoC.Archiver.LoadFile(path, model.Source, false);
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
