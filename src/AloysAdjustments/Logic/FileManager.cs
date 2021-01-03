using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Logic
{
    public class FileManager
    {
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

        public static async Task Cleanup(string tempPath)
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            });
        }
    }
}
