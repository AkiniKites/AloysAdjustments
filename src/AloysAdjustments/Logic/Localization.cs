using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using Decima;
using Decima.HZD;

namespace AloysAdjustments.Logic
{
    public class Localization
    {
        public ELanguage Language { get; }

        private readonly Dictionary<string, Dictionary<BaseGGUUID, string>> _cache = 
            new Dictionary<string, Dictionary<BaseGGUUID, string>>();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public Localization(ELanguage language)
        {
            Language = language;
        }
        
        public async Task<string> GetString(string file, BaseGGUUID id)
        {
            if (!_cache.TryGetValue(file, out var texts))
            {
                await _lock.WaitAsync();
                try
                {
                    if (!_cache.TryGetValue(file, out texts))
                    {
                        texts = await LoadFile(file);
                        _cache[file] = texts;
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }

            if (texts.TryGetValue(id, out var val))
                return val;
            return null;
        }

        private async Task<Dictionary<BaseGGUUID, string>> LoadFile(string path)
        {
            var core = await IoC.Archiver.LoadFileAsync(Configs.GamePackDir, path);
            var texts = new Dictionary<BaseGGUUID, string>();

            foreach (var obj in core.GetTypes<LocalizedTextResource>())
            {
                texts[obj.ObjectUUID] = obj.GetStringForLanguage(Language);
            }

            return texts;
        }
    }
}
