using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Utility;
using Decima;
using Decima.DS;
using FileMode = System.IO.FileMode;

namespace AloysAdjustments.Logic
{
    public class Archiver
    {
        private const string PatchPrefix = "Patch";
        private const string PackExt = ".bin";

        public System.Collections.Generic.HashSet<string> IgnoreList { get; }

        public Archiver(IEnumerable<string> ignoreList)
        {
            IgnoreList = ignoreList.ToHashSet(StringComparer.OrdinalIgnoreCase);
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

        public async Task ExtractFile(string dir, string source, string output)
        {
            ValidatePackager();

            await Task.Run(() =>
            {
                using var fs = File.OpenWrite(output);
                if (!TryExtractFile(dir, fs, source))
                    throw new HzdException($"Unable to extract file, file not found: {source}");
            });
        }
        public async Task<HzdCore> LoadFile(string dir, string file, bool throwError = true)
        {
            ValidatePackager();

            return await Task.Run(() =>
            {
                using var ms = new MemoryStream();
                if (!TryExtractFile(dir, ms, file))
                {
                    if (throwError)
                        throw new HzdException($"Unable to extract file, file not found: {file}");
                    return null;
                }

                ms.Position = 0;
                return HzdCore.Load(ms, file);
            });
        }

        private bool TryExtractFile(string path, Stream stream, string file)
        {
            List<string> packFiles;

            if (!file.EndsWith(".core", StringComparison.OrdinalIgnoreCase))
                file += ".core";

            if (Directory.Exists(path))
            {
                packFiles = GetPackFiles(path, PackExt);
                packFiles.RemoveAll(x => IgnoreList.Contains(Path.GetFileName(x)));
            }
            else if (File.Exists(path))
            {
                packFiles = new List<string>() { path };
            }
            else
            {
                throw new HzdException($"Unable to extract file, source path not found: {path}");
            }

            var hash = Packfile.GetHashForPath(file);
            var packMap = LoadPacks(packFiles, new[] { hash });

            if (!packMap.TryGetValue(hash, out var pack))
                return false;

            pack.ExtractFile(hash, stream);

            return true;
        }

        private List<string> GetPackFiles(string dir, string ext)
        {
            var files = Directory.GetFiles(dir, $"*{ext}", SearchOption.AllDirectories)
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

            return files;
        }

        private Dictionary<ulong, Packfile> LoadPacks(List<string> packFiles, IEnumerable<ulong> nameHashes)
        {
            var loadedPacks = new Dictionary<ulong, Packfile>();
            var hashes = nameHashes.ToHashSet();

            foreach (var packFile in packFiles)
            {
                var pack = new Packfile(packFile);
                try
                {
                    bool needed = false;
                    for (int i = 0; i < pack.FileEntries.Count; i++)
                    {
                        var hash = pack.FileEntries[i].PathHash;
                        if (hashes.Contains(hash))
                        {
                            if (loadedPacks.TryGetValue(hash, out var oldPack) && !ReferenceEquals(oldPack, pack))
                                oldPack.Dispose();
                            loadedPacks[hash] = pack;
                            needed = true;
                        }
                    }

                    if (!needed)
                        pack.Dispose();
                }
                catch
                {
                    pack.Dispose();

                    foreach (var loadedPacksValue in loadedPacks.Values)
                        loadedPacksValue.Dispose();

                    throw;
                }
            }

            return loadedPacks;
        }

        public async Task PackFiles(string dir, string output)
        {
            ValidatePackager();

            if (!Directory.Exists(dir))
                throw new HzdException($"Unable to create pack, directory not found: {dir}");

            dir = Path.GetFullPath(dir);
            output = Path.GetFullPath(output);

            await Task.Run(() =>
            {
                var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
                var fileNames = files.Select(x => x.Substring(dir.Length + 1).Replace("\\", "/")).ToArray();

                using (var pack = new Packfile(output, FileMode.Create))
                {
                    pack.BuildFromFileList(dir, fileNames);
                }
            });
        }
        
        public async Task GetLibrary()
        {
            var libPath = Path.Combine(IoC.Settings.GamePath, IoC.Config.ArchiverLib);
            if (!File.Exists(libPath))
                throw new HzdException($"Unable to find archiver support library in: {IoC.Settings.GamePath}");
            
            await Task.Run(() => File.Copy(libPath, IoC.Config.ArchiverLib, true));
        }
    }
}
