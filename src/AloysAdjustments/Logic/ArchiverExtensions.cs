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

namespace AloysAdjustments.Logic
{
    public static class ArchiverExtensions
    {
        public static async Task<HzdCore> LoadFileAsync(this Archiver archiver,
            string path, string file, bool throwError = false)
        {
            return await Async.Run(() => archiver.LoadFile(path, file, throwError));
        }

        public static async Task<HzdCore> LoadFileFromDirAsync(this Archiver archiver, 
            string dir, HashSet<string> fileFilter, string file)
        {
            return await Async.Run(() => archiver.LoadFileFromDir(dir, fileFilter, file));
        }
        
        public static async Task<HzdCore> LoadGameFileAsync(this Archiver archiver,
            string file)
        {
            return await Async.Run(() => LoadGameFile(archiver, file));
        }
        public static HzdCore LoadGameFile(this Archiver archiver, string file)
        {
            return archiver.LoadFileFromDir(Configs.GamePackDir,
                IoC.Config.KnownGameFiles.ToHashSet(), file);
        }
        public static MemoryStream LoadGameFileStream(this Archiver archiver, string file)
        {
            return archiver.LoadFileStreamFromDir(Configs.GamePackDir,
                IoC.Config.KnownGameFiles.ToHashSet(), file);
        }
    }
}