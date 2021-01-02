using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Utility;
using Decima;
using FileMode = System.IO.FileMode;

namespace AloysAdjustments.Logic
{
    public class Packager
    {
        public bool CheckPackagerLib()
        {
            return File.Exists(IoC.Config.PackagerLib);
        }

        public void ValidatePackager()
        {
            if (!CheckPackagerLib())
                throw new HzdException($"Packager support library not found: {IoC.Config.PackagerLib}");
        }

        public async Task ExtractFile(string dir, string source, string output)
        {
            //ValidateDecima();

            //dir = Path.GetFullPath(dir);
            //output = Path.GetFullPath(output);

            //var p = new ProcessRunner(IoC.Config.Decima.Path, $"-extract \"{dir}\" \"{source}\" \"{output}\"");
            //var result = await p.Run();
            //if (result.ExitCode != 0)
            //    throw new HzdException($"Unable to extract file '{source}' from '{dir}', error code: {result.ExitCode}");
        }

        public async Task PackFiles(string dir, string output)
        {
            ValidatePackager();

            if (!Directory.Exists(dir))
                throw new HzdException($"Unable to pack directory, doesn't exist: {dir}");

            dir = Path.GetFullPath(dir);
            output = Path.GetFullPath(output);

            await Task.Run(() =>
            {
                var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
                var fileNames = files.Select(x => x.Substring(dir.Length + 1).Replace("\\", "/")).ToArray();

                using (var pack = new Packfile(output, FileMode.Create))
                {
                    pack.BuildFromFileList(dir, fileNames);
                }
            });
        }
        
        public async Task GetLibrary()
        {
            var libPath = Path.Combine(IoC.Settings.GamePath, Path.GetFileName(IoC.Config.PackagerLib));
            if (!File.Exists(libPath))
                throw new HzdException($"Unable to find packager support library in: {IoC.Settings.GamePath}");

            Paths.CheckDirectory(Path.GetDirectoryName(IoC.Config.PackagerLib));
            await Task.Run(() => File.Copy(libPath, IoC.Config.PackagerLib, true));
        }
    }
}
