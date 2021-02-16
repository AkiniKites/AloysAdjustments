using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.Outfits.Data;
using HZDCoreEditor.Util;

namespace AloysAdjustments.Plugins.NPC.Characters
{
    public class ProgressReporter : IDisposable
    {
        private readonly int _maxValue;
        private readonly bool _hideOnComplete;
        private int _current;
        private int _lastNotify;
        private bool _disposed;

        public ProgressReporter (int maxValue, bool hideOnComplete = true)
        {
            _maxValue = maxValue;
            _hideOnComplete = hideOnComplete;
        }

        public void Tick()
        {
            if (_disposed)
                throw new Exception("ProgressReporter is disposed");

            Interlocked.Increment(ref _current);

            var notify = _current * 100 / _maxValue;
            if (notify> _lastNotify)
            {
                _lastNotify = notify;
                IoC.Notif.ShowProgress(notify / 100.0 / _maxValue);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                throw new Exception("ProgressReporter is already disposed");
            _disposed = true;

            if (_hideOnComplete)
                IoC.Notif.HideProgress();
            else
                IoC.Notif.ShowUnknownProgress();
        }
    }

    public class CharacterReferences
    {
        public Dictionary<Model, List<string>> FindReferences(IList<Model> models)
        {
            var files = Prefetch.Load().Files.Keys;

            var modelSearches = new List<(Model Model, BoyerMoore BM)>();
            foreach (var model in models)
                modelSearches.Add((model, new BoyerMoore(model.Id.ToBytes())));
            
            var modelBag = new ConcurrentBag<(string File, List<Model> Models)>();
            using var progress = new ProgressReporter(files.Count);

            var tasks = new ParallelTasks<string>(
                Environment.ProcessorCount, file =>
                {
                    var modelsInFile = SearchReferencesInFile(file, modelSearches);
                    if (modelsInFile.Any())
                        modelBag.Add((file, modelsInFile));

                    progress.Tick();
                });

            tasks.Start();
            tasks.AddItems(files);
            tasks.WaitForComplete();

            GC.Collect();

            var references = new Dictionary<Model, List<string>>();
            foreach (var found in modelBag)
            {
                foreach (var model in found.Models)
                {
                    if (!references.TryGetValue(model, out var foundFiles))
                    {
                        foundFiles = new List<string>();
                        references.Add(model, foundFiles);
                    }

                    foundFiles.Add(found.File);
                }
            }

            return references;
        }

        public List<Model> SearchReferencesInFile(string file, IList<(Model Model, BoyerMoore BM)> models)
        {
            using var ms = IoC.Archiver.LoadGameFileStream(file);
            var bytes = ms.ToArray();

            var found = new List<Model>();
            for (int i = 0; i < models.Count; i++)
            {
                var model = models[i];
                if (file == model.Model.Source)
                    continue;
                if (model.BM.Search(bytes) > 0)
                    found.Add(model.Model);
            }
            
            return found;
        }
    }
}
