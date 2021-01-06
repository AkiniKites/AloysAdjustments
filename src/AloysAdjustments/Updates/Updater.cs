using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Logic;
using AloysAdjustments.Utility;
using Onova;
using Onova.Models;
using Onova.Services;

namespace AloysAdjustments.Updates
{
    public class Updater
    {
        private const string UpdateDir = "update";
        private readonly Regex UpdateRepoMatcher = new Regex("^(?<user>.+?)[\\/](?<repo>.+)$");
        
        public CheckForUpdatesResult Status { get; private set; }
        public bool Prepared { get; private set; }

        public Updater()
        {
            Status = new CheckForUpdatesResult(new List<Version>(), null, false);
        }

        public async Task<CheckForUpdatesResult> CheckForUpdates()
        {
            using var manager = CreateUpdater();
            Status = await manager.CheckForUpdatesAsync();
            return Status;
        }

        public async Task PrepareUpdate(Action<double> progress = null)
        {
            var reporter = progress == null ? null : new Progress<double>(progress);

            using var manager = CreateUpdater();

            Status = await manager.CheckForUpdatesAsync();
            if (Status.CanUpdate)
            {
                await manager.PrepareUpdateAsync(Status.LastVersion, reporter);
                Prepared = true;
            }
        }

        public void TryLaunchUpdater(bool restart)
        {
            if (!Prepared || Status.LastVersion == null)
                return;

            using var manager = CreateUpdater();
            manager.LaunchUpdater(Status.LastVersion, restart);
        }

        public async Task Cleanup()
        {
            await Async.Run(() =>
            {
                try
                {
                    if (Directory.Exists(UpdateDir))
                        Directory.Delete(UpdateDir, true);
                }
                catch { } //ignore errors
            });
        }

        private UpdateManager CreateUpdater()
        {
            if (String.IsNullOrEmpty(IoC.Config.UpdatesRepo))
                throw new UpdateException("Update repo is null or empty");
            var repo = UpdateRepoMatcher.Match(IoC.Config.UpdatesRepo.Trim());
            if (!repo.Success)
                throw new UpdateException($"Update repo is not in the correct format: {IoC.Config.UpdatesRepo}");

            var meta = GetMetaData();

            var updater = new UpdateManager(
                meta,
                new GithubPackageResolver(repo.Groups["user"].Value, repo.Groups["repo"].Value, "*.zip"),
                new ReleaseExtractor());

            var updateDir = Path.GetFullPath(UpdateDir);


            //sketchy stuff
            SetPrivateField(updater, "_storageDirPath", updateDir);

            var updaterPath = GetPrivateField<string>(updater, "_updaterFilePath");
            updaterPath = Path.Combine(updateDir, Path.GetFileName(updaterPath));
            SetPrivateField(updater, "_updaterFilePath", updaterPath);

            var lockPath = GetPrivateField<string>(updater, "_lockFilePath");
            lockPath = Path.Combine(updateDir, Path.GetFileName(lockPath));
            SetPrivateField(updater, "_lockFilePath", lockPath);

            return updater;
        }

        private void SetPrivateField(UpdateManager updater, string name, object value)
        {
            typeof(UpdateManager)
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(updater, value);
        }
        private T GetPrivateField<T>(UpdateManager updater, string name)
        {
            return (T)typeof(UpdateManager)
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(updater);
        }

        private AssemblyMetadata GetMetaData()
        {
            var entry = Assembly.GetEntryAssembly();
            if (entry == null)
                throw new InvalidOperationException("Cannot get entry assembly");
            
            return new AssemblyMetadata(
                entry.GetName().Name, 
                entry.GetName().Version,
                Process.GetCurrentProcess().MainModule.FileName);
        }
    }
}
