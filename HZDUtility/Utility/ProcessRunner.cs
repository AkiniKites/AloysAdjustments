using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HZDUtility.Utility
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

        public async Task<(int ExitCode, string StdErr)> Run()
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

            using Process p = new Process { StartInfo = si };

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

            await WaitForExitAsync(p);

            return (p.ExitCode, esb.ToString());
        }

        public Task WaitForExitAsync(Process process)
        {
            if (process.HasExited) 
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);

            return process.HasExited ? Task.CompletedTask : tcs.Task;
        }
    }
}
