using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AloysAdjustments.Utility
{
    public class Errors
    {
        public static void WriteError(object exception)
        {
            try
            {
                File.AppendAllText("error.log", $"{exception}\r\n\r\n");
            }
            catch { }
        }
        public static void WriteError(Exception exception)
        {
            try
            {
                File.AppendAllText("error.log", $"{exception}\r\n\r\n");
            }
            catch { }
        }
    }
}
