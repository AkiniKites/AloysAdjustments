using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace AloysAdjustments.Configuration
{
    public class CmdOptions
    {
        [Option("no-cache", HelpText = "Disable all game caches")]
        public bool DisableGameCache { get; set; }

        [Option("single-thread", HelpText = "Run all parallel tasks in a single thread")]
        public bool SingleThread { get; set; }

        [Option("version", HelpText = "Set the program version")]
        public string Version { get; set; }

        [Option('p', "patch", HelpText = "Build patch and exit")]
        public bool BuildPatch { get; set; }
        
        [Option('c', "cmd", HelpText = "Execute commands automatically. Command format is '<plugin name> <command>'...")]
        public IEnumerable<string> Commands { get; set; }
    }
}
