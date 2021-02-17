using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AloysAdjustments.Logic;

namespace AloysAdjustments.Utility
{
    public class FileBackup : IDisposable
    {
        private const string GuidPattern = "[0-9A-F]{8}-(?:[0-9A-F]{4}-){3}[0-9A-F]{12}";

        private readonly string _path;
        private readonly string _tempPath;

        public static void CleanupBackups(string path)
        {
            foreach (var file in GetMatchingFiles(path))
                File.Delete(file);
        }

        public static async Task<bool> RunWithBackupAsync(string path, Func<bool> action)
        {
            return await Async.Run(() => RunWithBackup(path, action));
        }
        public static bool RunWithBackup(string path, Func<bool> action)
        {
            var success = false;

            try
            {
                if (File.Exists(path) && action())
                    success = true;
            }
            catch (Exception ex)
            {
                Errors.WriteError(ex);
            }

            var backupFiles = GetMatchingFiles(path).ToList();

            if (!success && backupFiles.Any())
            {
                if (File.Exists(path)) File.Delete(path);
                File.Move(backupFiles.First(), path);
                success = action();
            }

            foreach (var file in backupFiles)
            {
                if (File.Exists(file)) File.Delete(file);
            }

            return success;
        }

        private static IEnumerable<string> GetMatchingFiles(string path)
        {
            var filename = Path.GetFileName(path);
            var matcher = new Regex($"^{Regex.Escape(filename)}{GuidPattern}$", RegexOptions.IgnoreCase);
            var dir = Path.GetDirectoryName(path);
            if (String.IsNullOrEmpty(dir))
                dir = ".";

            if (Directory.Exists(dir))
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    if (matcher.IsMatch(Path.GetFileName(file)))
                        yield return file;
                }
            }
        }

        public FileBackup(string path)
        {
            _path = path;

            try
            {
                if (File.Exists(path))
                {
                    _tempPath = path + IoC.Uuid.New();
                    File.Move(path, _tempPath);
                }
            }
            catch(IOException ex)
            {
                throw new Exception($"Failed to rename file: {path}", ex);
            }
        }

        public void Delete()
        {
            if (_tempPath != null && File.Exists(_tempPath))
                File.Delete(_tempPath);
        }

        public void Dispose()
        {
            if (_tempPath != null && File.Exists(_tempPath))
            {
                if (File.Exists(_path)) File.Delete(_path);
                File.Move(_tempPath, _path);
            }
        }
    }
}
