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
            new Version(1,7,2)
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
            
            //prev setting was before versions so assume true
            if (String.IsNullOrEmpty(version) || !Version.TryParse(version, out var prevVersion))
                return true;

            //can't get current version, shouldn't happen
            var curVersion = GetVersion();
            if (curVersion == null || curVersion.Equals(prevVersion))
                return true;

            //prev version before or at breakpoint and current version after
            foreach (var v in breakpoints)
            {
                if (prevVersion <= v && curVersion >= v)
                    return true;
            }

            return false;
        }

        public static Version GetVersion()
        {
            var entry = Assembly.GetEntryAssembly();
            return entry?.GetName().Version;
        }
    }
}
