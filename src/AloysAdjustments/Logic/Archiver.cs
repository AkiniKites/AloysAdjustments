using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Utility;
using Decima;
using Utility;

namespace AloysAdjustments.Logic
{
    public class Archiver
    {
        private const string PatchPrefix = "Patch";
        private const string PackExt = ".bin";

        private readonly ConcurrentDictionary<PackList, Dictionary<ulong, string>> _packFileLocator;
        private readonly ConcurrentDictionary<string, PackfileReader> _packCache;

        public Archiver()
        {
            _packFileLocator = new ConcurrentDictionary<PackList, Dictionary<ulong, string>>();
            _packCache = new ConcurrentDictionary<string, PackfileReader>(StringComparer.OrdinalIgnoreCase);
        }

        public bool CheckArchiverLib()
        {
            return File.Exists(IoC.Config.ArchiverLib);
        }
        public void ValidatePackager()
        {
            if (!CheckArchiverLib())
                throw new HzdException($"Packager support library not found: {IoC.Config.ArchiverLib}");
        }
        public async Task GetLibrary()
        {
            var libPath = Path.Combine(IoC.Settings.GamePath, IoC.Config.ArchiverLib);
            if (!File.Exists(libPath))
                throw new HzdException($"Unable to find archiver support library in: {IoC.Settings.GamePath}");

            await Async.Run(() =>
            {
                OodleLZ.Unload();
                File.Copy(libPath, IoC.Config.ArchiverLib, true);
            });
        }

        public void ClearCache()
        {
            _packFileLocator.Clear();
            _packCache.Clear();
        }
        
        public HzdCore LoadFile(string path, string file, bool throwError = false)
        {
            ValidatePackager();

            using var ms = new MemoryStream();
            if (!TryExtractFile(path, ms, file))
            {
                if (throwError)
                    throw new HzdException($"Unable to extract file, file not found: {file}");
                return null;
            }

            ms.Position = 0;
            return HzdCore.FromStream(ms, file);
        }
        private bool TryExtractFile(string path, Stream stream, string file)
        {
            if (!File.Exists(path))
                throw new HzdException($"Unable to extract file, source path not found: {path}");

            var pack = LoadPack(path, false);
            var hash = Packfile.GetHashForPath(file);
            if (pack.FileEntries.All(x => x.PathHash != hash))
                return false;

            pack.ExtractFile(hash, stream);
            return true;
        }
        
        public HzdCore LoadFileFromDir(string dir, HashSet<string> fileFilter, string file)
        {
            ValidatePackager();

            using var ms = new MemoryStream();
            if (!TryExtractFileFromDir(dir, fileFilter, ms, file))
                throw new HzdException($"Unable to extract file, file not found: {file}");

            ms.Position = 0;
            return HzdCore.FromStream(ms, file);
        }
        private bool TryExtractFileFromDir(string dir, HashSet<string> fileFilter, Stream stream, string file)
        {
            if (!Directory.Exists(dir))
                throw new HzdException($"Unable to extract file, directory not found: {dir}");

            var packs = GetPackFiles(dir, fileFilter, PackExt);
            var fileMap = BuildFileMap(packs, true);
            
            var hash = Packfile.GetHashForPath(file);
            if (!fileMap.TryGetValue(hash, out var packFile))
                return false;

            var pack = LoadPack(packFile, true);
            pack.ExtractFile(hash, stream);

            return true;
        }

        private PackList GetPackFiles(string dir, HashSet<string> fileFilter, string ext)
        {
            var files = Directory.GetFiles(dir, $"*{ext}", SearchOption.AllDirectories)
                .Where(x=> fileFilter == null || fileFilter.Contains(Path.GetFileName(x)))
                .OrderBy(x => x).ToList();

            //move patch files to end, same as game load order
            int moved = 0;
            for (int i = 0; i < files.Count - moved; i++)
            {
                if (Path.GetFileName(files[i]).StartsWith(PatchPrefix))
                {
                    var file = files[i];
                    files.RemoveAt(i);
                    files.Add(file);

                    i--;
                    moved++;
                }
            }

            return new PackList(files);
        }
        private Dictionary<ulong, string> BuildFileMap(PackList packFiles, bool useCache)
        {
            if (useCache && _packFileLocator.TryGetValue(packFiles, out var files))
                return files;

            files = new Dictionary<ulong, string>();

            foreach (var packFile in packFiles.Packs)
            {
                var pack = LoadPack(packFile, useCache);
                for (int i = 0; i < pack.FileEntries.Count; i++)
                {
                    var hash = pack.FileEntries[i].PathHash;
                    files[hash] = packFile;
                }
            }

            if (useCache)
                _packFileLocator.TryAdd(packFiles, files);
            return files;
        }

        public void PackFiles(string dir, string[] files, string output)
        {
            ValidatePackager();

            if (!Directory.Exists(dir))
                throw new HzdException($"Unable to create pack, directory not found: {dir}");

            var pack = new PackfileWriterFast(output, false, true);
            pack.BuildFromFileList(dir, files);
        }

        private PackfileReader LoadPack(string path, bool useCache)
        {
            if (!useCache)
                return new PackfileReader(path);

            path = Path.GetFullPath(path);
            if (!_packCache.TryGetValue(path, out var pack))
            {
                pack = new PackfileReader(path);
                _packCache.TryAdd(path, pack);
            }
            
            return pack;
        }
    }
}
