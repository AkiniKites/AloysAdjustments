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

        /// <summary>
        /// Extract a core file to it's relative path
        /// </summary>
        public static async Task<HzdCore> ExtractFile(
            string extractPath, string pakPath, string file)
        {
            if (!Directory.Exists(pakPath) && !File.Exists(pakPath))
                throw new HzdException($"Pack file or directory not found at: {pakPath}");
            
            if (!file.EndsWith(".core", StringComparison.OrdinalIgnoreCase))
                file += ".core";

            string output = Path.Combine(extractPath, file);
            Paths.CheckDirectory(Path.GetDirectoryName(output));
            
            await IoC.Archiver.ExtractFile(pakPath, file, output);

            return HzdCore.Load(output, file);
        }

        public static async Task InstallPatch(string patchFile, string packDir)
        {
            if (!File.Exists(patchFile))
                throw new HzdException($"Patch file not found at: {patchFile}");
            if (!Directory.Exists(packDir))
                throw new HzdException($"Pack directory not found at: {packDir}");

            var dest = Path.Combine(packDir, Path.GetFileName(patchFile));

            await Task.Run(() => File.Copy(patchFile, dest, true));
        }

        public static async Task Cleanup(string path)
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            });
        }

        public static async Task CleanupFile(string path, bool tempFilesOnly)
        {
            await Task.Run(() =>
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
