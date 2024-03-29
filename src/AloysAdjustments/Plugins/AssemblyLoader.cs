﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using AloysAdjustments.Logic;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Plugins
{
    public class AssemblyLoader
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static List<T> LoadTypes<T>(string path)
        {
            var loaded = new List<T>();

            path = Path.GetFullPath(path);

            var assembly = Assembly.Load(File.ReadAllBytes(path));
            var assemblyLoaderType = assembly.GetType("Costura.AssemblyLoader", false);
            var attachMethod = assemblyLoaderType?.GetMethod("Attach", BindingFlags.Static | BindingFlags.Public);
            attachMethod?.Invoke(null, new object[] { });

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
                Errors.WriteError(ex);
            }

            return loaded;
        }
    }
}
