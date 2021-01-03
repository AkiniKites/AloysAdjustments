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
        public static async Task<string> ExtractFile(string extractPath, 
            string pakPath, bool retainPaths, string file)
        {
            if (!Directory.Exists(pakPath) && !File.Exists(pakPath))
                throw new HzdException($"Pack file or directory not found at: {pakPath}");

            Paths.CheckDirectory(extractPath);

            if (!file.EndsWith(".core", StringComparison.OrdinalIgnoreCase))
                file += ".core";

            string output;
            if (retainPaths)
            {
                output = Path.Combine(extractPath, file);
                Paths.CheckDirectory(Path.GetDirectoryName(output));
            }
            else
            {
                var ext = Path.GetExtension(file);
                output = Path.Combine(extractPath, Guid.NewGuid() + ext);
            }
            
            await IoC.Archiver.ExtractFile(pakPath, file, output);

            return output;
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
