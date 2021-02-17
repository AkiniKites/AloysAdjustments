using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Steam;
using AloysAdjustments.Utility;
using CommandLine;
using Decima.HZD;
using String = System.String;

namespace AloysAdjustments.Tools
{
    public class CmdOptions
    {
        [Option('c', "characters", HelpText = "Build character cache")]
        public string CharacterDir { get; set; }

        [Option('g', "game", HelpText = "Game directory for getting oodle library")]
        public string GameDir { get; set; }
    }

    class Program
    {
        private const string SteamGameName = "Horizon Zero Dawn";

        static void Main(string[] args)
        {
            var cmds = new CmdOptions();
            var parser = new Parser(with => with.HelpWriter = Console.Error);
            parser.ParseArguments<CmdOptions>(args)
                .WithParsed(o => cmds = o)
                .WithNotParsed(errs => Console.WriteLine("Unable to parse command line: {0}", String.Join(" ", args)));

            Configs.LoadConfigs();
            IoC.Bind(new Uuid());
            IoC.Bind(new Archiver());
            IoC.Bind(new Localization(ELanguage.English));

            if (!CheckArchiver(cmds.GameDir))
                return;

            if (cmds.CharacterDir != null)
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
                new NpcRefGenerator().SearchDir(cmds.CharacterDir);
            }
        }

        private static bool CheckArchiver(string gameDir)
        {
            if (IoC.Archiver.CheckArchiverLib())
                return true;

            if (String.IsNullOrEmpty(gameDir))
            {
                gameDir = new GameSearch().FindSteamGameDir(SteamGameName);
                if (String.IsNullOrEmpty(gameDir))
                {
                    Console.WriteLine("Cannot get oodle library, Cannot find game and no game directory specified.");
                    return false;
                }
            }

            try
            {
                IoC.Archiver.GetLibrary().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot get oodle library, {ex.Message}.");
                return false;
            }

            return true;
        }
    }
}
