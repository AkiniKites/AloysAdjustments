using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Plugins.NPC;
using AloysAdjustments.Plugins.NPC.Characters;
using AloysAdjustments.Plugins.Outfits;
using AloysAdjustments.Plugins.Outfits.Data;
using AloysAdjustments.Utility;
using HZDCoreEditor.Util;
using Newtonsoft.Json;

namespace AloysAdjustments.Tools
{
    public class CharacterReferences
    {
        private const string ResultsFile = "results.json";
        private const string ReferencesFile = "references.json";

        public CharacterReferences()
        {
            IoC.Bind(Configs.LoadModuleConfig<OutfitConfig>("NPC Models"));
        }

        public void SearchDir(string path)
        {
            var search = new ByteSearch(path, LoadResults()?.Select(x => x.File));
            
            var charGen = new CharacterGenerator();
            var models = charGen.GetCharacterModels(true);
            models.AddRange(charGen.GetCharacterModels(false));

            var modelPatterns = models.Select(x => (x, x.Id.ToBytes())).ToList();
            
            var processed = new BlockingCollection<ByteSearchResult<CharacterModel>>();
            TrackProgress(processed);

            search.Search(modelPatterns, result =>
            {
                foreach (var item in result.Results)
                    Console.WriteLine($"{item.Key} -> {result.File}");
                processed.Add(result);
            });

            processed.CompleteAdding();

            var results = LoadResults().Where(x => x.Found);
            var refs = new Dictionary<Model, HashSet<string>>();
            foreach (var result in results)
            {
                foreach (var entry in result.Results)
                {
                    if (!refs.TryGetValue(entry.Key, out var files))
                    {
                        files = new HashSet<string>();
                        refs.Add(entry.Key, files);
                    }

                    files.Add(result.File.Substring(path.Length).Replace("\\", "/"));
                }
            }

            var references = refs.Select(x => (x.Key.Source, x.Key.Name, x.Value));
            var json = JsonConvert.SerializeObject(references, Formatting.Indented);
            File.WriteAllText(ReferencesFile, json);
        }

        private List<ByteSearchResult<CharacterModel>> LoadResults()
        {
            List<ByteSearchResult<CharacterModel>> results = null;
            FileBackup.RunWithBackup(ResultsFile, () =>
            {
                if (!File.Exists(ResultsFile))
                    return false;
                var json = File.ReadAllText(ResultsFile);
                results = JsonConvert.DeserializeObject<List<ByteSearchResult<CharacterModel>>>(json, new BaseGGUUIDConverter());
                return true;
            });

            return results;
        }
        private void SaveResults(List<ByteSearchResult<CharacterModel>> results)
        {
            var json = JsonConvert.SerializeObject(results, 
                Formatting.Indented, new BaseGGUUIDConverter());
            using (var fb = new FileBackup(ResultsFile))
            {
                File.WriteAllText(ResultsFile, json);
                fb.Delete();
            }
        }

        private void TrackProgress(BlockingCollection<ByteSearchResult<CharacterModel>> processed)
        {
            Task.Run(() =>
            {
                var results = LoadResults() ?? new List<ByteSearchResult<CharacterModel>>();

                foreach (var file in processed.GetConsumingEnumerable())
                {
                    results.Add(file);
                    if (results.Count % 100 == 0)
                        SaveResults(results);
                }

                SaveResults(results);
            });
        }
    }
}
