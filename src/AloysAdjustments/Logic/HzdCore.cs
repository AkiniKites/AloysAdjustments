using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;

namespace AloysAdjustments.Logic
{
    public class HzdCore
    {
        public string Source { get; protected set; }
        public CoreBinary Binary { get; protected set; }

        public static HzdCore FromFile(string path, string source)
        {
            using var fs = File.OpenRead(path);
            return HzdCore.FromStream(fs, source);
        }
        public static HzdCore FromStream(Stream stream, string source)
        {
            try
            {
                using var br = new BinaryReader(stream, Encoding.UTF8, true);

                return new HzdCore()
                {
                    Source = NormalizeSource(source),
                    Binary = CoreBinary.FromData(br, true)
                };
            }
            catch (Exception ex)
            {
                throw new HzdException($"Failed to load: {source ?? "null"}", ex);
            }
        }

        public virtual void Save()
        {
            throw new NotSupportedException();
        }

        public static string NormalizeSource(string path)
        {
            if (Path.GetExtension(path) == Packfile.CoreExt)
                path = path.Substring(0, path.Length - Packfile.CoreExt.Length);
            return path.Replace("\\", "/");
        }

        public static string EnsureExt(string file)
        {
            return Packfile.EnsureExt(file);
        }
    }
}
