﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic.Patching;
using HZDCoreEditor.Util;

namespace AloysAdjustments.Utility
{
    public class FileCollector<T>
    {
        private readonly Func<string, bool> _fileNameValidator;
        private readonly Func<string, IEnumerable<T>> _itemGetter;
        private readonly HashSet<string> Ignored;

        private readonly GameCache<List<T>> _cache;
        private readonly object _lock = new object();

        public FileCollector(string name, Func<string, bool> fileNameValidator, 
            Func<string, IEnumerable<T>> itemGetter, string[] ignored = null)
        {
            _fileNameValidator = fileNameValidator;
            _itemGetter = itemGetter;

            Ignored = new HashSet<string>(
                ignored ?? new string[0], StringComparer.OrdinalIgnoreCase);
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

            var tasks = new ParallelTasks<string>(
                Environment.ProcessorCount, file =>
                {
                    if (!Ignored.Contains(file) && _fileNameValidator(file))
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