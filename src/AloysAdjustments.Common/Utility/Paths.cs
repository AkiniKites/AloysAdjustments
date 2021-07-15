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
        
        public static void Cleanup(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            var dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: " + sourceDirName);
            }
   
            CheckDirectory(destDirName);

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            if (copySubDirs)
            {
                foreach (var subDir in dir.GetDirectories())
                {
                    var tempPath = Path.Combine(destDirName, subDir.Name);
                    DirectoryCopy(subDir.FullName, tempPath, copySubDirs);
                }
            }
        }

        public static long Size(string path)
        {
            if (File.Exists(path))
                return new FileInfo(path).Length;
            return DirectorySize(new DirectoryInfo(path));
        }
        public static long DirectorySize(DirectoryInfo dir)
        {
            long size = 0;

            foreach (var fi in dir.GetFiles())
                size += fi.Length;
            foreach (var di in dir.GetDirectories())
                size += DirectorySize(di);

            return size;
        }
    }
}
