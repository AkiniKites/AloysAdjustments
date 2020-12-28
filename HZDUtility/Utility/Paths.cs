using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HZDUtility.Utility
{
    public static class Paths
    {
        public static void CheckDirectory(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}
