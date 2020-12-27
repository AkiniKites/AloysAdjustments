using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HZDUtility
{
    public class FileManager
    {
        public const string PakFolder = "Packed_DX12";

        public static async Task<Dictionary<string, string>> ExtractFiles(
            string decimaPath, string tempPath, string gamePath, bool retainPath,
            params string[] files)
        {
            CheckExists(decimaPath);

            CheckDirectory(tempPath);

            var tempFiles = new Dictionary<string, string>();

            var dir = Path.Combine(gamePath, PakFolder);

            foreach (var f in files)
            {
                string output;
                if (retainPath)
                {
                    output = Path.Combine(tempPath, f);
                    CheckDirectory(Path.GetDirectoryName(output));
                }
                else
                {
                    var ext = Path.GetExtension(f);
                    output = Path.Combine(tempPath, Guid.NewGuid() + ext);
                }
                
                await ExtractFile(decimaPath, dir, f, output);

                tempFiles.Add(f, output);
            }

            return tempFiles;
        }

        private static async Task ExtractFile(
            string decimaPath, string dir, string source, string output)
        {
            var p = new ProcessRunner(decimaPath, $"-extract \"{dir}\" \"{source}\" \"{output}\"");
            var result = await p.RunAsync();
            if (result.ExitCode != 0)
                throw new Exception($"Unable to extract file '{source}' from '{dir}', error code: {result.ExitCode}");
        }


        public static async Task PackFiles(string decimaPath, string dir, string output)
        {
            CheckExists(decimaPath);
            if (!Directory.Exists(dir))
                throw new Exception($"Unable to pack directory, doesn't exist: {dir}");
            
            var p = new ProcessRunner(decimaPath, $"-pack \"{dir}\" \"{output}\"");
            var result = await p.RunAsync();
            if (result.ExitCode != 0)
                throw new Exception($"Unable to pack dir '{dir}' to '{output}', error code: {result.ExitCode}");
        }

        public static async Task Cleanup(string tempPath)
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            });
        }

        private static void CheckDirectory(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private static void CheckExists(string file)
        {
            if (!File.Exists(file))
            {
                throw new Exception($"Unable to find: {file}");
            }
        }
    }
}
