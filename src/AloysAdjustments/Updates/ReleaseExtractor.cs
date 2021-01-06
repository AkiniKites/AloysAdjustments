using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AloysAdjustments.Utility;
using Onova.Services;

namespace AloysAdjustments.Updates
{
    public class ReleaseExtractor : IPackageExtractor
    {
        public async Task ExtractPackageAsync(string sourceFilePath, string destDirPath, 
            IProgress<double> progress = null, CancellationToken cancellationToken = new CancellationToken())
        {
            await Async.Run(() =>
            {
                ExtractRelease(sourceFilePath, destDirPath);
            }, cancellationToken);
        }

        private void ExtractRelease(string sourceFilePath, string destDirPath)
        {
            using var archive = ZipFile.OpenRead(sourceFilePath);

            var root = archive.Entries.FirstOrDefault();
            if (!IsDir(root?.FullName))
                throw new UpdateException("Failed to update, zip does not contain root folder");

            var rootLen = root.FullName.Length;

            foreach (var entry in archive.Entries.Skip(1))
            {
                var destPath = Path.Combine(destDirPath, entry.FullName.Substring(rootLen));
                
                if (IsDir(destPath))
                {
                    Paths.CheckDirectory(destPath);
                    continue;
                }

                Paths.CheckDirectory(Path.GetDirectoryName(destPath));

                entry.ExtractToFile(destPath);
            }
        }

        private bool IsDir(string path)
        {
            if (path == null)
                return false;

            return path.Last() == Path.DirectorySeparatorChar ||
                path.Last() == Path.AltDirectorySeparatorChar;
        }
    }
}
