using AloysAdjustments.Logic;
using System;
using System.IO;
using System.Threading.Tasks;
using AloysAdjustments.Common.Downloads;
using HZDCoreEditor.Util;

namespace AloysAdjustments.Plugins.Common
{
    public class ModelImageRepo
    {
        public const string ModelImageExt = ".jpg";
        
        private readonly TestDownloader _downloader = new TestDownloader(10);
        
        private string GetFileName(string modelName)
        {
            return Path.Combine(IoC.Config.ImageCachePath, modelName + ModelImageExt);
        }
        
        public void LoadImage(string modelName, Action<bool, byte[]> callback)
        {
            DownloadModelImage(modelName, callback);
        }

        private void DownloadModelImage(string modelName, Action<bool, byte[]> callback)
        {
            var path = GetFileName(modelName);
            _downloader.Download(modelName, path, (success, downloaded, bytes) =>
            {
                if (downloaded) IoC.Notif.CacheUpdate();
                callback(success, bytes);
            });
        }
    }
}
