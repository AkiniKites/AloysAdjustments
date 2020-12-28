using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HZDUtility.Utility;

namespace HZDUtility
{
    public class FileManager
    {
        public static async Task<(string Source, string Output)> ExtractFile(
            Decima decima, string tempPath, string path, bool retainPath, string file)
        {
            return (await ExtractFiles(decima, tempPath, path, retainPath, file)).FirstOrDefault();
        }

        public static async Task<List<(string Source, string Output)>> ExtractFiles(
            Decima decima, string tempPath, string path, bool retainPath, params string[] files)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
                throw new HzdException($"Pack file/directory not found at: {path}");

            Paths.CheckDirectory(tempPath);

            var tempFiles = new List<(string Source, string Output)>();
            
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
                
                await decima.ExtractFile(path, f, output);

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
