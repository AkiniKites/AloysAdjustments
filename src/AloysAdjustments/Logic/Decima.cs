using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Utility;
using Octokit;

namespace AloysAdjustments.Logic
{
    public class Decima
    {
        public bool CheckDecimaExe()
        {
            return File.Exists(IoC.Config.Decima.Path);
        }
        public bool CheckDecimaLib()
        {
            return File.Exists(IoC.Config.Decima.Lib);
        }

        public void ValidateDecima()
        {
            if (!File.Exists(IoC.Config.Decima.Path))
                throw new HzdException($"Decima executable not found: {IoC.Config.Decima.Path}");
            if (!File.Exists(IoC.Config.Decima.Lib))
                throw new HzdException($"Decima support library not found: {IoC.Config.Decima.Lib}");
        }
        public async Task Download()
        {
            var client = new GitHubClient(new ProductHeaderValue("none"));

            var releases = await client.Repository.Release.GetAll(IoC.Config.Decima.RepoUser, IoC.Config.Decima.RepoName);
            var latest = releases.FirstOrDefault();
            if (latest == null)
                throw new HzdException($"Unable to find latest release for: {IoC.Config.Decima.RepoUser}//{IoC.Config.Decima.RepoName}");

            var file = latest.Assets.FirstOrDefault(x => x.Name == IoC.Config.Decima.RepoFile);
            if (file == null)
                throw new HzdException($"Unable to find release file: {IoC.Config.Decima.RepoFile}");

            Paths.CheckDirectory(Path.GetDirectoryName(IoC.Config.Decima.Path));

            using (var wc = new WebClient())
                await wc.DownloadFileTaskAsync(new Uri(file.BrowserDownloadUrl), IoC.Config.Decima.Path);
        }
        public async Task GetLibrary()
        {
            var libPath = Path.Combine(IoC.Settings.GamePath, Path.GetFileName(IoC.Config.Decima.Lib));
            if (!File.Exists(libPath))
                throw new HzdException($"Unable to find decima support library in: {IoC.Settings.GamePath}");
            
            Paths.CheckDirectory(Path.GetDirectoryName(IoC.Config.Decima.Lib));
            await Task.Run(() => File.Copy(libPath, IoC.Config.Decima.Lib, true));
        }
    }
}
