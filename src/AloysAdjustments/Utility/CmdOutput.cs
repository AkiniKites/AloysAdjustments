using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Utility
{
    public class CmdOutput
    {
        public const string CommandOutputFile = "output.txt";

        public static void Reset()
        {
            if (File.Exists(CommandOutputFile))
                File.Delete(CommandOutputFile);
        }

        public static void WriteLine(string text)
        {
            text ??= "";
            File.AppendAllText(CommandOutputFile, text);
        }
    }
}
