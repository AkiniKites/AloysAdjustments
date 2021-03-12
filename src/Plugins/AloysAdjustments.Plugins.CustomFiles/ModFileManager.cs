using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Plugins.CustomFiles.Configuration;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Plugins.CustomFiles
{
    public class ModFileManager
    {
        private const string ModDir = "mods";

        private ModFileLoader Loader { get; set; }
        public List<Mod> Mods { get; set; }

        public ModFileManager()
        {
            Loader = new ModFileLoader();
        }

        public async Task Initialize()
        {
            await Async.Run(Loader.Initialize);

            Mods = IoC.Get<ModSettings>().Mods;

            for (int i = 0; i < Mods.Count; i++)
            {
                var loaded = Loader.LoadModData(Mods[i]);
                if (loaded == null)
                {
                    Mods.RemoveAt(i);
                    i--;
                    continue;
                }
                
                Mods[i] = loaded;
            }

            Mods.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        public Mod AddFile(string path)
        {
            if (!Loader.ValidatePath(path))
                throw new Exception($"Mod file is not a valid format {path}");

            var modPath = CopyFile(path);
            var mod = Loader.LoadPath(modPath);

            if (mod == null)
            {
                if (File.Exists(modPath))
                    File.Delete(modPath);
                throw new Exception($"Mod file is not a valid format {path}");
            }

            AddMod(mod);
            return mod;
        }

        public Mod AddFolder(string path)
        {
            if (!Loader.ValidatePath(path))
                throw new Exception($"Mod folder is not a valid format {path}");

            var modPath = CopyFolder(path);
            var mod = Loader.LoadPath(modPath);

            if (mod == null)
            {
                if (Directory.Exists(modPath))
                    Directory.Delete(modPath, true);
                throw new Exception($"Mod folder is not a valid format {path}");
            }

            AddMod(mod);
            return mod;
        }

        private void AddMod(Mod mod)
        {
            Mods.Add(mod);
            Settings.Save();
        }

        private string CopyFile(string path)
        {
            var name = Path.GetFileName(path);
            var modPath = Path.Combine(ModDir, name);

            if (File.Exists(modPath) || Directory.Exists(modPath))
                throw new Exception($"Mod already exists with the same name {name}");

            Paths.CheckDirectory(Path.GetDirectoryName(modPath));
            File.Copy(path, modPath);

            return modPath;
        }

        public string CopyFolder(string path)
        {
            var name = Path.GetFileName(path);
            var modPath = Path.Combine(ModDir, name);

            if (File.Exists(modPath) || Directory.Exists(modPath))
                throw new Exception($"Mod already exists with the same name {name}");

            Paths.CheckDirectory(Path.GetDirectoryName(modPath));
            Paths.DirectoryCopy(path, modPath);

            return modPath;
        }
    }
}
