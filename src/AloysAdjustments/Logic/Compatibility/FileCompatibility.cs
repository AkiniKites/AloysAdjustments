﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Utility;
using Onova.Models;

namespace AloysAdjustments.Logic
{
    public class FileCompatibility
    {
        private const string VersionObjectName = "AA-Version";

        public static bool ShouldMigrate(string path, Version version)
        {
            return GetPreviousVersion(path) < version;
        }

        public static void RunMigrations(string path, List<(Version Version, Action Action)> migrations)
        {
            var prevVersion = GetPreviousVersion(path);

            foreach (var migration in migrations.OrderBy(x => x.Version))
            {
                if (prevVersion < migration.Version)
                    migration.Action();
            }
        }

        private static Version GetPreviousVersion(string path)
        {
            var prevVersion = ObjectStore.LoadObject<Version>(path, VersionObjectName);
            var curVersion = IoC.CurrentVersion;

            //prev setting was before versions so assume true
            //can't get current version, shouldn't happen
            if (prevVersion == null ||
                curVersion == null)
            {
                return new Version();
            }

            return prevVersion;
        }

        public static void SaveVersion(Patch patch)
        {
            var curVersion = IoC.CurrentVersion;
            if (curVersion != null)
                patch.AddObject(curVersion, VersionObjectName);
        }
    }
}
