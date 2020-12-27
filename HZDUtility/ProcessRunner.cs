using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace HZDUtility
{
    public class ProcessRunner
    {
        public ProcessRunner(string cmd, string args)
        {
            Command = cmd;
            Arguments = args;
        }

        public string Command { get; }
        public string Arguments { get; }

        public (int ExitCode, string StdErr) Run()
        {
            var si = new ProcessStartInfo()
            {
                FileName = Command,
                Arguments = Arguments,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process p = new Process())
            {
                p.StartInfo = si;

                var osb = new StringBuilder();
                var esb = new StringBuilder();

                p.OutputDataReceived += (s, e) => {
                    osb.AppendLine(e.Data);
                };
                p.ErrorDataReceived += (s, e) => {
                    esb.AppendLine(e.Data);
                };

                p.Start();

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                p.WaitForExit();

                return (p.ExitCode, esb.ToString());
            }
        }

        public async Task<(int ExitCode, string StdErr)> RunAsync()
        {
            return await Task.Run(() => Run());
        }
    }
}
