using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Utility
{
    public static class Paths
    {
        public static void CheckDirectory(string dir)
        {
            if (String.IsNullOrEmpty(dir))
                return;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}
