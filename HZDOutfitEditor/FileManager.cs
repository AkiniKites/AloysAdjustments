using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HZDOutfitEditor.Utility;

namespace HZDOutfitEditor
{
    public class FileManager
    {
        public static async Task<(string Source, string Output)> ExtractFile(
            Decima decima, string extractPath, string pakPath, bool retainPath, string file)
        {
            return (await ExtractFiles(decima, extractPath, pakPath, retainPath, file)).FirstOrDefault();
        }

        public static async Task<List<(string Source, string Output)>> ExtractFiles(
            Decima decima, string extractPath, string pakPath, bool retainPath, params string[] files)
        {
            if (!Directory.Exists(pakPath) && !File.Exists(pakPath))
                throw new HzdException($"Pack file/directory not found at: {pakPath}");

            Paths.CheckDirectory(extractPath);

            var tempFiles = new List<(string Source, string Output)>();

            foreach (var temp in files)
            {
                var f = temp;
                if (!f.EndsWith(".core", StringComparison.OrdinalIgnoreCase))
                    f += ".core";

                string output;
                if (retainPath)
                {
                    output = Path.Combine(extractPath, f);
                    Paths.CheckDirectory(Path.GetDirectoryName(output));
                }
                else
                {
                    var ext = Path.GetExtension(f);
                    output = Path.Combine(extractPath, Guid.NewGuid() + ext);
                }
                
                await decima.ExtractFile(pakPath, f, output);

                if (!File.Exists(output))
                    throw new HzdException($"Failed to extract: {f}");

                tempFiles.Add((f, output));
            }

            return tempFiles;
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
