using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;

namespace AloysAdjustments.Plugins
{
    public class PluginManager
    {
        private readonly HashSet<string> _ignoreList = new[] { 
            IoC.Config.ArchiverLib 
        }.ToHashSet();

        public void ExecuteAll<T>(string dir, Action<T> action)
        {
            var dlls = Directory.GetFiles(dir, "*.dll", SearchOption.TopDirectoryOnly);

            foreach (string dll in dlls)
            {
                if (_ignoreList.Contains(Path.GetFileName(dll)))
                    continue;

                LoadAndExecute(dll, action);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void LoadAndExecute<T>(string path, Action<T> action)
        {
            try
            {
                path = Path.GetFullPath(path);
                var alc = new AssemblyLoadContext(path, true);
                var a = alc.LoadFromAssemblyPath(path);
                
                try
                {
                    var types = a.GetTypes().Where(x => typeof(T).IsAssignableFrom(x));
                    foreach (var t in types)
                    {
                        var obj = (T)Activator.CreateInstance(t);
                        action(obj);
                    }
                }
                catch
                {
                    //TODO: implement
                }

                alc.Unload();
            }
            catch
            {
                //TODO: implement
            }
        }
    }
}
