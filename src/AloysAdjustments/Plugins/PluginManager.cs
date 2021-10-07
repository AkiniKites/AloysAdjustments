using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Common.Utility;
using AloysAdjustments.Logic;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Plugins
{
    public class PluginManager
    {
        private const string PluginDir = "plugins";
        private const string PluginPattern = "*.dll";
        
        public IEnumerable<T> LoadAll<T>()
        {
            Paths.CheckDirectory(PluginDir);
            var dlls = Directory.GetFiles(PluginDir, PluginPattern, SearchOption.TopDirectoryOnly);

            foreach (string dll in dlls)
            {
                foreach (var loadedType in AssemblyLoader.LoadTypes<T>(dll))
                {
                    yield return loadedType;
                }
            }
        }
    }
}
