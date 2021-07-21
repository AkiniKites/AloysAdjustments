using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using HZDCoreEditor.Util;

namespace AloysAdjustments.Utility
{
    public class FileCollector<T>
    {
        private readonly Func<string, bool> _fileNameValidator;
        private readonly Func<string, IEnumerable<T>> _itemGetter;
        private readonly Regex[] Ignored;

        private readonly GameCache<List<T>> _cache;
        private readonly object _lock = new object();

        public FileCollector(string name, Func<string, bool> fileNameValidator, 
            Func<string, IEnumerable<T>> itemGetter, string[] ignored = null)
        {
            _fileNameValidator = fileNameValidator;
            _itemGetter = itemGetter;

            Ignored = (ignored ?? new string[0]).Select(x => new Regex(x, RegexOptions.IgnoreCase)).ToArray();
            _cache = new GameCache<List<T>>(name);
        }

        public List<T> Load()
        {
            lock (_lock)
            {
                return LoadInternal();
            }
        }
        private List<T> LoadInternal()
        {
            if (_cache.TryLoadCache(out var cached))
            {
                if (cached.Any())
                    return cached;
            }
            else
            {
                cached = new List<T>();
            }

            var files = Prefetch.Load().Files.Keys;
            var bag = new ConcurrentBag<T>();

            var parallels = IoC.CmdOptions.SingleThread ? 1 : Environment.ProcessorCount;
            var tasks = new ParallelTasks<string>(
                parallels, file =>
                {
                    if (_fileNameValidator(file) && !Ignored.Any(x => x.IsMatch(file)))
                    {
                        foreach (var item in _itemGetter(file))
                            bag.Add(item);
                    }
                });

            tasks.Start();
            tasks.AddItems(files);
            tasks.WaitForComplete();

            GC.Collect();

            var itemList = bag.ToList();
            cached.AddRange(itemList);
            _cache.Save(cached);

            return itemList;
        }
    }
}
