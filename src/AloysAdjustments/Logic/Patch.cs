using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Logic
{
    public class Patch
    {
        public string WorkingDir { get; }
        public string PackedFile { get; set; }

        public Patch(string workingDir)
        {
            WorkingDir = workingDir;
        }

        public HzdCore AddFile(string file)
        {
            var filePath = HzdCore.EnsureExt(file);

            string output = Path.Combine(WorkingDir, filePath);

            if (!File.Exists(output))
            {
                Paths.CheckDirectory(Path.GetDirectoryName(output));

                IoC.Archiver.ExtractFile(Configs.GamePackDir, filePath, output);
            }

            return HzdCore.Load(output, file);
        }
    }
}
