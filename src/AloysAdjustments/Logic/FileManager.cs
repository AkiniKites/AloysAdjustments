using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Logic
{
    public class FileManager
    {
        private const string GuidPattern = "[0-9A-F]{8}-(?:[0-9A-F]{4}-){3}[0-9A-F]{12}";
        
        public static async Task Cleanup(string path)
        {
            await Async.Run(() =>
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            });
        }

        public static async Task CleanupFile(string path, bool tempFilesOnly)
        {
            await Async.Run(() =>
            {
                var filename = Path.GetFileName(path);
                var matcher = new Regex($"^{Regex.Escape(filename)}{GuidPattern}$", RegexOptions.IgnoreCase);
                foreach (var file in Directory.GetFiles(Path.GetDirectoryName(path)))
                {
                    if (matcher.IsMatch(Path.GetFileName(file)))
                        File.Delete(file);
                }

                if (!tempFilesOnly && File.Exists(path))
                    File.Delete(path);
            });
        }
    }
}
