using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima.HZD;

namespace AloysAdjustments.Plugins.CustomFiles
{
    public class ModLogic
    {
        private const string ModDir = "mods";

        public void AddFile(string path)
        {
            var name = Path.GetFileName(path);
            var modPath = Path.Combine(ModDir, name);

            if (File.Exists(modPath))
                throw new Exception($"Mod already exists with the same name {name}");

            File.Copy(path, modPath);


        }
    }
}
