using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace AloysAdjustments.Steam
{
    public class GameSearch
    {
        public string FindSteamGameDir(string gameName)
        {
            foreach (var steamDir in GetSteamGameFolders())
            {
                try
                {
                    var gameDir = Path.Combine(steamDir, gameName);
                    if (Directory.Exists(gameDir))
                        return gameDir;
                }
                catch { } //ignore
            }

            return null;
        }

        private List<string> GetSteamGameFolders()
        {
            const string steam32 = "SOFTWARE\\VALVE\\";
            const string steam64 = "SOFTWARE\\Wow6432Node\\Valve\\";

            try
            {
                var key64 = Registry.LocalMachine.OpenSubKey(steam64);
                if (String.IsNullOrEmpty(key64?.ToString()))
                {
                    var key32 = Registry.LocalMachine.OpenSubKey(steam32);
                    return ReadValueRegKey(key32);
                }

                return ReadValueRegKey(key64);
            }
            catch { } //ignore errors

            return new List<string>();
        }

        private List<string> ReadValueRegKey(RegistryKey key)
        {
            var dirs = new List<string>();

            try
            {
                foreach (string valveSubKey in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(valveSubKey);

                    var steamPath = subKey?.GetValue("InstallPath")?.ToString();
                    if (String.IsNullOrEmpty(steamPath))
                        continue;

                    var configPath = steamPath + "/steamapps/libraryfolders.vdf";
                    const string driveRegex = @"[A-Z]:\\";

                    if (File.Exists(configPath))
                    {
                        var configLines = File.ReadAllLines(configPath);
                        foreach (var line in configLines)
                        {
                            var match = Regex.Match(line, driveRegex);
                            if (line != string.Empty && match.Success)
                            {
                                var path = line.Substring(line.IndexOf(match.ToString()));
                                path = path.Replace("\\\\", "\\");
                                path = path.Replace("\"", "\\steamapps\\common\\");
                                dirs.Add(path);
                            }
                        }
                        dirs.Add(steamPath + "\\steamapps\\common\\");
                    }
                }
            }
            catch { } //ignore errors

            return dirs;
        }
    }
}
