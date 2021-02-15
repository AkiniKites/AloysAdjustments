using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.Outfits.Data;
using HZDCoreEditor.Util;

namespace AloysAdjustments.Plugins.NPC.Characters
{
    public class CharacterReferences
    {
        public void FindReferences(IList<Model> models)
        {
            var files = Prefetch.Load().Files.Keys;

            var modelBag = new ConcurrentBag<CharacterModel>();

            var tasks = new ParallelTasks<string>(
                Environment.ProcessorCount, file =>
                {
                    SearchReferencesInFile(file, models);
                });

            tasks.Start();
            tasks.AddItems(files);
            tasks.WaitForComplete();

            GC.Collect();
        }

        public bool SearchReferencesInFile(string file, IList<Model> models)
        {
            var pack = IoC.Archiver.LoadGameFile(file);
            return true;
        }
    }
}
