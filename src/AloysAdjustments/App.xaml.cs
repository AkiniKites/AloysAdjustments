using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloysAdjustments.Logic;
using CommandLine;

namespace AloysAdjustments
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var cmds = new CmdOptions();
            var parser = new Parser(with => with.HelpWriter = Console.Error);
            parser.ParseArguments<CmdOptions>(e.Args)
                .WithParsed(o => cmds = o)
                .WithNotParsed(errs => Console.WriteLine("Unable to parse command line: {0}", String.Join(" ", e.Args)));

            IoC.Debug.DisableGameCache = cmds.DisableGameCache;
            IoC.Debug.SingleThread = cmds.SingleThread;

            base.OnStartup(e);
        }
    }

    public class CmdOptions
    {
        [Option("disable-game-cache", HelpText = "Disable all game caches")]
        public bool DisableGameCache { get; set; }

        [Option("single-thread", HelpText = "Run all parallel tasks in a single thread")]
        public bool SingleThread { get; set; }
    }
}
