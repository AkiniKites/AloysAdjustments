using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Utility;
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

        //only supports english right now
        private TextInfo _textInfo = new CultureInfo("en-US",false).TextInfo;


        public Localization(ELanguage language)
        {
            Language = language;
        }
        
        public string ToTitleCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return _textInfo.ToTitleCase(text);
        }

        public async Task<string> GetString(LocalString text)
        {
            if (text == null) return string.Empty;
            return await GetString(text.File, text.Id);
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
            var core = await IoC.Archiver.LoadGameFileAsync(path);
            var texts = new Dictionary<BaseGGUUID, string>();

            foreach (var obj in core.GetTypes<LocalizedTextResource>())
            {
                texts[obj.ObjectUUID] = obj.GetStringForLanguage(Language).Trim();
            }

            return texts;
        }
    }
}
