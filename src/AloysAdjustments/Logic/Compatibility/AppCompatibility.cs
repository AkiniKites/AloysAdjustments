﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Common.Utility;
using AloysAdjustments.Configuration;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Logic.Compatibility
{
    public class AppCompatibility
    {
        private static readonly List<(Version Version, Action Action)> _migrations;

        static AppCompatibility()
        {
            _migrations = new List<(Version, Action)>();

            _migrations.Add((new Version(1, 0, 0), CleanupOldVersions));
            _migrations.Add((new Version(1, 7, 4), CleanCache));
            _migrations.Add((new Version(1, 7, 5), CleanCache));
        }

        public static bool ShouldMigrate(Version version)
        {
            return GetPreviousVersion() < version;
        }

        public static void AddMigration(Version version, Action action)
        {
            _migrations.Add((version, action));
        }

        public static void RunMigrations()
        {
            var prevVersion = GetPreviousVersion();
            var curVersion = IoC.CurrentVersion ?? new Version();

            foreach (var migration in _migrations.OrderBy(x => x.Version))
            {
                if (prevVersion < migration.Version && curVersion >= migration.Version)
                    migration.Action();
            }
        }

        private static Version GetPreviousVersion()
        {
            var version = IoC.Settings.Version;
            var curVersion = IoC.CurrentVersion;

            //prev setting was before versions so assume true
            //can't get current version, shouldn't happen
            if (String.IsNullOrEmpty(version)
                || !Version.TryParse(version, out var prevVersion)
                || curVersion == null)
            {
                return new Version();
            }

            return prevVersion;
        }

        public static void CleanupOldVersions()
        {
            foreach (var fileName in IoC.Config.OldVersionsToDelete)
            {
                var path = Path.Combine(Configs.GamePackDir, fileName);

                FileBackup.CleanupBackups(path);
                if (File.Exists(path)) File.Delete(path);
            }
        }

        public static void CleanCache()
        {
            Paths.DeleteDirectory(IoC.Config.CachePath);
        }
    }
}
