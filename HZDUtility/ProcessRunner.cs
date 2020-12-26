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

        public int Run()
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = Command;
                p.StartInfo.Arguments = Arguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                p.OutputDataReceived += (sender, e) => {
                    outputBuilder.AppendLine(e.Data);
                };
                p.ErrorDataReceived += (sender, e) => {
                    errorBuilder.AppendLine(e.Data);
                };

                p.Start();

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                p.WaitForExit();

                return p.ExitCode;
            }
        }

        public async Task<int> RunAsync()
        {
            return await Task.Run(() => RunAndWait());
        }
    }
}
