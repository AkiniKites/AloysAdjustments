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
        public const string ModelDbId = "[id]";
        public const string ModelImageExt = ".jpg";
        
        private readonly HttpDownloader _downloader = new HttpDownloader(10);
        
        private string GetFileName(string url)
        {
            return Path.Combine(IoC.Config.ImagesPath, Path.GetFileName(url));
        }
        private string GetDownloadUrl(string modelName)
        {
            return IoC.Config.ImagesDb.Replace(ModelDbId, modelName);
        }

        public void LoadImage(string modelName, Action<bool, byte[]> callback)
        {
            DownloadModelImage(modelName, callback);
        }

        private void DownloadModelImage(string modelName, Action<bool, byte[]> callback)
        {
            var url = GetDownloadUrl(modelName);
            var path = GetFileName(url);

            _downloader.Download(url, path, (success, downloaded, bytes) =>
            {
                if (downloaded) IoC.Notif.CacheUpdate();
                callback(success, bytes);
            });
        }
    }
}
