using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;

namespace AloysAdjustments.Logic
{
    public class Prefetch
    {
        public HzdCore Core { get; private set; }
        public Dictionary<string, int> Files { get; private set; }
        public PrefetchList Data { get; private set; }

        public static async Task<Prefetch> LoadAsync()
        {
            return await Async.Run(Load);
        }
        public static Prefetch Load()
        {
            var prefetch = new Prefetch();

            prefetch.Core = IoC.Archiver.LoadFile(Configs.GamePackDir, IoC.Config.PrefetchFile);
            prefetch.Data = prefetch.Core.GetTypes<PrefetchList>().First();

            prefetch.Files = new Dictionary<string, int>();
            for (int i = 0; i < prefetch.Data.Files.Count; i++)
                prefetch.Files.Add(prefetch.Data.Files[i].Path.Value, i);

            return prefetch;
        }

        private Prefetch() { }
        
        public async Task Save(string path)
        {
            Paths.CheckDirectory(Path.GetDirectoryName(path));
            await Core.Save(path);
        }

        public async Task<bool> Rebuild(string patchDir)
        {
            return await Async.Run(() => {
                var changed = false;
                var dirLen = Path.GetFullPath(patchDir).Length + 1;
                var links = GetLinks();
                
                foreach (var f in new DirectoryInfo(patchDir).GetFiles("*", SearchOption.AllDirectories))
                {
                    var name = f.FullName.Substring(dirLen).Replace(".core", "").Replace("\\", "/");
                    
                    if (Files.TryGetValue(name, out int idx))
                    {
                        if (Data.Sizes[idx] != (int)f.Length)
                        {
                            changed = true;
                            Data.Sizes[idx] = (int)f.Length;
                        }

                        UpdateLinks(links, f.FullName, name);
                    }
                }

                RebuildLinks(links);

                return changed;
            });
        }

        private int[][] GetLinks()
        {
            var fileLinks = new int[Data.Files.Count][];

            int linkIdx = 0;

            for (int i = 0; i < Data.Files.Count; i++)
            {
                int count = Data.Links[linkIdx];
                var links = new int[count];

                Data.Links.CopyTo(linkIdx + 1, links, 0, count);
                fileLinks[i] = links;

                linkIdx += count + 1;
            }

            return fileLinks;
        }

        private void UpdateLinks(int[][] fileLinks, string filepath, string name)
        {
            var fileCore = HzdCore.Load(filepath, name);

            // Regenerate links for this specific file (don't forget to remove duplicates (Distinct()!!!))
            var newLinks = fileCore.Binary.GetAllReferences()
                .Where(x => x.Type == BaseRef.Types.ExternalCoreUUID)
                .Select(x => Files[x.ExternalFile.Value])
                .Distinct()
                .ToArray();

            fileLinks[Files[name]] = newLinks;
        }

        private void RebuildLinks(int[][] fileLinks)
        {
            // Dictionary of links -> linear array
            Data.Links.Clear();

            for (int i = 0; i < Data.Files.Count; i++)
            {
                var indices = fileLinks[i];
                Data.Links.Add(indices.Length);

                foreach (int index in indices)
                    Data.Links.Add(index);
            }
        }
    }
}
