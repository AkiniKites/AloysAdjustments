using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HZDUtility.Utility;

namespace HZDUtility
{
    public class FileManager
    {
        public static async Task<Dictionary<string, string>> ExtractFiles(
            Decima dm, string tempPath, string packDir, bool retainPath,
            params string[] files)
        {
            if (!Directory.Exists(packDir))
                throw new HzdException($"Pack directory not found at: {packDir}");

            Paths.CheckDirectory(tempPath);

            var tempFiles = new Dictionary<string, string>();
            
            foreach (var f in files)
            {
                string output;
                if (retainPath)
                {
                    output = Path.Combine(tempPath, f);
                    Paths.CheckDirectory(Path.GetDirectoryName(output));
                }
                else
                {
                    var ext = Path.GetExtension(f);
                    output = Path.Combine(tempPath, Guid.NewGuid() + ext);
                }
                
                await dm.ExtractFile(packDir, f, output);

                tempFiles.Add(f, output);
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

            await Task.Run(() => File.Copy(patchFile, dest));
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
