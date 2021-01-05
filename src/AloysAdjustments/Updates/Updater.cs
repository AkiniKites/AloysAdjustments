using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Onova;
using Onova.Models;
using Onova.Services;

namespace AloysAdjustments.Updates
{
    public class Updater
    {
        private const string UpdateDir = "update";

        public async Task<CheckForUpdatesResult> CheckForUpdates()
        {
            using var manager = CreateUpdater();
            return await manager.CheckForUpdatesAsync();
        }

        public async Task PerformUpdate()
        {
            using var manager = CreateUpdater();
            await manager.CheckPerformUpdateAsync();
        }

        public async Task Cleanup()
        {
            await Task.Run(() =>
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
            var updater = new UpdateManager(
                new GithubPackageResolver("akinikites", "AloysAdjustments", "*.zip"),
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
    }
}
