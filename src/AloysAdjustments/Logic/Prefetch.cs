using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using Decima.HZD;

namespace AloysAdjustments.Logic
{
    public static class Prefetch
    {
        public static async Task<Dictionary<string, int>> LoadPrefetchAsync()
        {
            return await Task.Run(LoadPrefetch);
        }
        public static Dictionary<string, int> LoadPrefetch()
        {
            var core = IoC.Archiver.LoadFile(Configs.GamePackDir, IoC.Config.PrefetchFile);
            var data = core.GetTypes<PrefetchList>().Values.First();

            var files = new Dictionary<string, int>();
            for (int i = 0; i < data.Files.Count; i++)
                files.Add(data.Files[i].Path.Value, i);
            return files;
        }

        public static async Task RebuildPrefetch(string patchDir)
        {
            var prefetchFile = Path.GetFileName(IoC.Config.PrefetchFile);

            var core = await FileManager.ExtractFile(patchDir,
                Configs.GamePackDir, IoC.Config.PrefetchFile);

            var data = core.GetTypes<PrefetchList>().Values.First();

            var files = new Dictionary<string, int>();
            for (int i = 0; i < data.Files.Count; i++)
                files[data.Files[i].Path.Value] = i;

            var changed = false;
            var dirLen = Path.GetFullPath(patchDir).Length + 1;
            foreach (var f in new DirectoryInfo(patchDir).GetFiles("*", SearchOption.AllDirectories))
            {
                if (f.FullName.Contains(prefetchFile))
                    continue;
                var name = f.FullName.Substring(dirLen).Replace(".core", "").Replace("\\", "/");
                if (data.Sizes[files[name]] != (int)f.Length)
                    changed = true;
                data.Sizes[files[name]] = (int)f.Length;
            }

            if (changed)
                core.Save();
            else
                File.Delete(core.FilePath);
        }
    }
}
