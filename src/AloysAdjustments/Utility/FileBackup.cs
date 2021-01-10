using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AloysAdjustments.Utility
{
    public class FileBackup : IDisposable
    {
        private const string GuidPattern = "[0-9A-F]{8}-(?:[0-9A-F]{4}-){3}[0-9A-F]{12}";

        private readonly string _path;
        private readonly string _tempPath;

        public static async Task CleanupBackups(string path)
        {
            await Async.Run(() =>
            {
                foreach (var file in GetMatchingFiles(path))
                    File.Delete(file);
            });
        }

        public static async Task<bool> RestoreBackup(string path)
        {
            return await Async.Run(() =>
            {
                var backup = GetMatchingFiles(path).FirstOrDefault();
                if (backup != null)
                    File.Move(backup, path, true);

                return backup != null;
            });
        }

        private static IEnumerable<string> GetMatchingFiles(string path)
        {
            var filename = Path.GetFileName(path);
            var matcher = new Regex($"^{Regex.Escape(filename)}{GuidPattern}$", RegexOptions.IgnoreCase);
            foreach (var file in Directory.GetFiles(Path.GetDirectoryName(path)))
            {
                if (matcher.IsMatch(Path.GetFileName(file)))
                    yield return file;
            }
        }

        public FileBackup(string path)
        {
            _path = path;

            try
            {
                if (File.Exists(path))
                {
                    _tempPath = path + Guid.NewGuid();
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
                File.Move(_tempPath, _path, true);
        }
    }
}
