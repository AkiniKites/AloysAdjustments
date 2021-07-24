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
        private Regex[] _ignored;
        private Func<IEnumerable<T>, IEnumerable<T>> _consolidator;

        private bool _sealed;
        private readonly GameCache<List<T>> _cache;
        private readonly object _lock = new object();

        public FileCollector(string name, 
            Func<string, bool> fileNameValidator, 
            Func<string, IEnumerable<T>> itemGetter)
        {
            _fileNameValidator = fileNameValidator;
            _itemGetter = itemGetter;
            _ignored = new Regex[0];

            _cache = new GameCache<List<T>>(name);
        }

        public FileCollector<T> WithIgnored(string[] ignored)
        {
            if (_sealed) throw new InvalidOperationException("FileCollector has been built cannot add ignored.");
            _ignored = ignored.Select(x => new Regex(x, RegexOptions.IgnoreCase)).ToArray();
            return this;
        }
        public FileCollector<T> WithConsolidate(Func<IEnumerable<T>, IEnumerable<T>> consolidator)
        {
            if (_sealed) throw new InvalidOperationException("FileCollector has been built cannot add consolidator.");
            _consolidator = consolidator;
            return this;
        }
        public FileCollector<T> Build()
        {
            _sealed = true;
            return this;
        }

        public List<T> Load()
        {
            if (!_sealed) throw new InvalidOperationException("FileCollector has not been built");

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
                    if (_fileNameValidator(file) && !_ignored.Any(x => x.IsMatch(file)))
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
            if (_consolidator != null)
                itemList = _consolidator(itemList).ToList();

            cached.AddRange(itemList);
            _cache.Save(cached);

            return itemList;
        }
    }
}
