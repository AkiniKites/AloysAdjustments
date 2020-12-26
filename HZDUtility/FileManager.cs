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
            string decimaPath, string tempPath, string gamePath, params string[] files)
        {
            CheckDirectory(tempPath);

            var tempFiles = new Dictionary<string, string>();

            var dir = Path.Combine(gamePath, PakFolder);

            foreach (var f in files)
            {
                var ext = Path.GetExtension(f);

                var extracted = Path.Combine(tempPath, Guid.NewGuid().ToString() + ext);

                var p = new ProcessRunner(decimaPath, $"-extract \"{dir}\" \"{f}\" \"{extracted}\"");
                var error = await p.RunAsync();
                if (error != 0)
                    throw new Exception($"Unable to extract file '{f}' from '{dir}', error code: {error}");

                tempFiles.Add(f, extracted);
            }

            return tempFiles;
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
    }
}
