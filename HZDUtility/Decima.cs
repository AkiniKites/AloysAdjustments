using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HZDUtility.Utility;
using Octokit;

namespace HZDUtility
{
    public class Decima
    {
        public Config Config { get; }

        public Decima(Config config)
        {
            Config = config;
        }

        public void CheckDecima()
        {
            if (!File.Exists(Config.Decima.Path))
                throw new HzdException($"Decima executable not found: {Config.Decima.Path}");
            if (!File.Exists(Config.Decima.Lib))
                throw new HzdException($"Decima support library not found: {Config.Decima.Lib}");
        }

        public async Task ExtractFile(string dir, string source, string output)
        {
            dir = Path.GetFullPath(dir);
            output = Path.GetFullPath(output);

            var p = new ProcessRunner(Config.Decima.Path, $"-extract \"{dir}\" \"{source}\" \"{output}\"");
            var result = await p.RunAsync();
            if (result.ExitCode != 0)
                throw new HzdException($"Unable to extract file '{source}' from '{dir}', error code: {result.ExitCode}");
        }


        public async Task PackFiles(string dir, string output)
        {
            if (!Directory.Exists(dir))
                throw new HzdException($"Unable to pack directory, doesn't exist: {dir}");

            dir = Path.GetFullPath(dir);
            output = Path.GetFullPath(output);

            var p = new ProcessRunner(Config.Decima.Path, $"-pack \"{dir}\" \"{output}\"");
            var result = await p.RunAsync();
            if (result.ExitCode != 0)
                throw new HzdException($"Unable to pack dir '{dir}' to '{output}', error code: {result.ExitCode}");
        }

        public async Task Download()
        {
            var client = new GitHubClient(new ProductHeaderValue("none"));

            var releases = await client.Repository.Release.GetAll(Config.Decima.RepoUser, Config.Decima.RepoName);
            var latest = releases.FirstOrDefault();
            if (latest == null)
                throw new HzdException($"Unable to find latest release for: {Config.Decima.RepoUser}//{Config.Decima.RepoName}");

            var file = latest.Assets.FirstOrDefault(x => x.Name == Config.Decima.RepoFile);
            if (file == null)
                throw new HzdException($"Unable to find release file: {Config.Decima.RepoFile}");

            Paths.CheckDirectory(Path.GetDirectoryName(Config.Decima.Path));

            using (var wc = new WebClient())
                await wc.DownloadFileTaskAsync(new Uri(file.BrowserDownloadUrl), Config.Decima.Path);
        }
        public async Task GetLibrary()
        {
            var libPath = Path.Combine(Config.Settings.GamePath, Path.GetFileName(Config.Decima.Lib));
            if (!File.Exists(libPath))
                throw new HzdException($"Unable to find decima support library in: {Config.Settings.GamePath}");
            
            Paths.CheckDirectory(Path.GetDirectoryName(Config.Decima.Lib));
            await Task.Run(() => File.Copy(libPath, Config.Decima.Lib, true));
        }
    }
}
