using AloysAdjustments.Logic;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AloysAdjustments.Common;
using AloysAdjustments.Common.Utility;

namespace AloysAdjustments.Plugins.Common
{
    public class ModelImageRepo
    {
        public const string ModelImageExt = ".jpg";
        
        private readonly Downloader _downloader = new Downloader();

        public void LoadImage(string modelName, Action<byte[]> callback)
        {
            DownloadModelImage(modelName, callback);
        }

        private string GetFileName(string modelName)
        {
            return Path.Combine(IoC.Config.ImageCachePath, modelName + ModelImageExt);
        }

        private void DownloadModelImage(string modelName, Action<byte[]> callback)
        {
            var path = GetFileName(modelName);
            _downloader.Download(modelName, path, (success, bytes) =>
            {
                if (success)
                    callback(bytes);
            });
        }

    }
}
