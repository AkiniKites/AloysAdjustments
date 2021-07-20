using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Utility;
using Onova.Models;

namespace AloysAdjustments.Logic
{
    public class Compatibility
    {
        private static readonly Version[] CleanCacheBreakpoints = { 
            new Version(1,7,4)
        };

        public static void CleanupOldVersions()
        {
            foreach (var fileName in IoC.Config.OldVersionsToDelete)
            {
                var path = Path.Combine(Configs.GamePackDir, fileName);

                FileBackup.CleanupBackups(path);
                if (File.Exists(path)) File.Delete(path);
            }
        }

        public static void CleanupOldCache()
        {
            //clean cache on specific versions
            if (HitBreakpoint(CleanCacheBreakpoints))
                Paths.Cleanup(IoC.Config.CachePath);
        }

        private static bool HitBreakpoint(Version[] breakpoints)
        {
            var version = IoC.Settings.Version;
            var curVersion = IoC.CurrentVersion;

            //prev setting was before versions so assume true
            //can't get current version, shouldn't happen
            if (String.IsNullOrEmpty(version) 
                || !Version.TryParse(version, out var prevVersion)
                || curVersion == null)
                return true;

            if (curVersion.Equals(prevVersion))
                return false;

            //prev version before breakpoint and current version after or at
            foreach (var v in breakpoints)
            {
                if (prevVersion < v && curVersion >= v)
                    return true;
            }

            return false;
        }
    }
}
