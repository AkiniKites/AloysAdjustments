using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Configuration;
using AloysAdjustments.Data;
using AloysAdjustments.Logic;
using AloysAdjustments.Utility;
using Decima.HZD;
using Newtonsoft.Json;

namespace AloysAdjustments.Modules.Outfits
{
    public class CharacterModelSearch
    {
        private readonly Regex HumanoidMatcher;
        private readonly Regex UniqueHumanoidMatcher;
        private readonly string[] Ignored;

        private readonly GameCache<(bool All, List<CharacterModel> Models)> _cache;

        public CharacterModelSearch()
        {
            HumanoidMatcher = new Regex(IoC.Get<OutfitConfig>().HumanoidMatcher);
            UniqueHumanoidMatcher = new Regex(IoC.Get<OutfitConfig>().UniqueHumanoidMatcher);
            Ignored = IoC.Get<OutfitConfig>().IgnoredCharacters.ToArray();

            _cache = new GameCache<(bool All, List<CharacterModel> Models)>("characters");
        }

        public async Task<List<CharacterModel>> GetCharacterModels(bool all = false)
        {
            var models = await Async.Run(() =>
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

                var files = Prefetch.LoadPrefetch().Keys;
                
                int progress = 0;
                int lastProgress = 0;
                var modelBag = new ConcurrentBag<CharacterModel>();

                Parallel.ForEach(files, file =>
                {
                    if (IsValid(file, all))
                    {
                        foreach (var model in GetCharacterModels(file))
                            modelBag.Add(model);
                    }
                    
                    //rough progress estimate
                    var newProgress = Interlocked.Increment(ref progress) * 50 / files.Count;
                    if (newProgress > lastProgress)
                    {
                        lastProgress = newProgress;
                        IoC.Notif.ShowProgress(newProgress, 50);
                    }
                });

                IoC.Notif.ShowUnknownProgress();

                var modelList = modelBag.ToList();
                _cache.Save((all, modelList));

                return modelList;
            });

            return models;
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
            var pack = IoC.Archiver.LoadFile(Configs.GamePackDir, file);
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
    }
}
