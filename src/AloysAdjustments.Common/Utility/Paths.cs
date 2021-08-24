using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AloysAdjustments.Common.Utility
{
    public static class Paths
    {
        public static long GetDirectorySize(string dir)
        {
            var dirInfo = new DirectoryInfo(dir);
            if (dirInfo.Exists)
                return dirInfo.GetFiles("*.*", SearchOption.AllDirectories).Sum(x => x.Length);
            return 0;
        }

        public static void CheckDirectory(string dir)
        {
            if (String.IsNullOrEmpty(dir))
                return;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        
        public static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path))
                return;
            DeleteDirectoryNoCheck(path);

            //wait up to 1s for dir to be deleted
            //if not just ignore it
            for (int i = 0; i < 100; i++)
            {
                if (!Directory.Exists(path))
                    return;

                Thread.Sleep(10);
            }
        }

        private static void DeleteDirectoryNoCheck(string path)
        {
            File.SetAttributes(path, FileAttributes.Normal);

            foreach (string file in Directory.GetFiles(path))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in Directory.GetDirectories(path))
                DeleteDirectory(dir);

            Directory.Delete(path, false);
        }
    }
} 
