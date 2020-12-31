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
            var file = await FileManager.ExtractFile(IoC.Decima,
                IoC.Config.TempPath, Configs.GamePackDir, false, path);

            var core = HzdCore.Load(file.Output);
            var texts = new Dictionary<BaseGGUUID, string>();

            foreach (var obj in core.Components)
            {
                if (obj is LocalizedTextResource asResource)
                    texts[asResource.ObjectUUID] = asResource.GetStringForLanguage(Language);
            }

            await FileManager.Cleanup(IoC.Config.TempPath);

            return texts;
        }
    }
}
