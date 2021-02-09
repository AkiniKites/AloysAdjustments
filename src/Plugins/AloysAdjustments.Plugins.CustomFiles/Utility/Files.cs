using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.CustomFiles.Utility
{
    public static class Files
    {
        public static string Normalize(string path)
        {
            return path.Replace("\\", "/");
        }

        public static bool IsDir(string path)
        {
            if (path == null)
                return false;

            return path.Last() == Path.DirectorySeparatorChar ||
                path.Last() == Path.AltDirectorySeparatorChar;
        }
    }
}
