using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using AloysAdjustments.Logic;

namespace AloysAdjustments.Plugins
{
    public class AssemblyLoader
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static List<T> LoadTypes<T>(string path)
        {
            var loaded = new List<T>();

            Assembly AssemblyResolve(object sender, ResolveEventArgs args)
            {
                var name = args.Name.Remove(args.Name.IndexOf(',')) + ".dll";

                var supportPath = Path.Combine(Path.GetDirectoryName(path), name);
                if (File.Exists(supportPath))
                    return Assembly.LoadFile(supportPath);
                return null;
            }

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            try
            {
                path = Path.GetFullPath(path);
                var assembly = Assembly.LoadFile(path);

                try
                {
                    var types = assembly.GetTypes().Where(x => typeof(T).IsAssignableFrom(x) && !x.IsAbstract);
                    foreach (var type in types)
                    {
                        var constructor = type.GetConstructor(Type.EmptyTypes);

                        //TODO: log errors for invalid types
                        if (constructor != null)
                            loaded.Add((T)Activator.CreateInstance(type));
                    }
                }
                catch (Exception ex)
                {
                    //TODO: implement
                }
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
            }

            return loaded;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoadAndExecute<T>(string path, Action<T> action)
        {
            try
            {
                path = Path.GetFullPath(path);
                var alc = new AssemblyLoadContext(path, true);
                var assembly = alc.LoadFromAssemblyPath(path);

                try
                {
                    var types = assembly.GetTypes().Where(x => typeof(T).IsAssignableFrom(x) && !x.IsAbstract);
                    foreach (var type in types)
                    {
                        var constructor = type.GetConstructor(Type.EmptyTypes);

                        //TODO: log errors for invalid types
                        if (constructor != null)
                        {
                            var obj = (T)constructor.Invoke(null, null);
                            action(obj);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO: implement
                }

                alc.Unload();
            }
            catch (Exception ex)
            {
                //TODO: implement
            }
        }
    }
}
