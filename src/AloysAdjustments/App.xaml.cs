using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloysAdjustments.Configuration;
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
            var dl = new AloysAdjustments.Common.Downloader();
            for (int i = 0; i < 100; i++)
            {
                dl.Download(i.ToString(), null, b => { });
            }

            var cmds = new CmdOptions();
            var parser = new Parser(with => with.HelpWriter = Console.Error);
            parser.ParseArguments<CmdOptions>(e.Args)
                .WithParsed(o => cmds = o)
                .WithNotParsed(errs => Console.WriteLine("Unable to parse command line: {0}", String.Join(" ", e.Args)));

            if (cmds.Commands.ToList().Count % 2 != 0)
                throw new ArgumentException("Invalid number of commands: " + cmds.Commands.ToList().Count);

            IoC.Bind(cmds);

            if (!String.IsNullOrEmpty(cmds.Version)
                && Version.TryParse(cmds.Version, out var debugVersion))
            {
                IoC.Bind(debugVersion);
            }

            base.OnStartup(e);
        }
    }
}
