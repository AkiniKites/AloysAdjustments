using AloysAdjustments.Logic;
using System;
using System.IO;
using System.Threading.Tasks;
using AloysAdjustments.Common.Downloads;

namespace AloysAdjustments.Plugins.Common
{
    public class ModelImageRepo
    {
        public const string ModelImageExt = ".jpg";
        
        private readonly TestDownloader _downloader = new TestDownloader(10);
        
        public async Task<byte[]> LoadImageAsync(string modelName)
        {
            return await Task.Run(() => DownloadModelImage(modelName));
        }

        private string GetFileName(string modelName)
        {
            return Path.Combine(IoC.Config.ImageCachePath, modelName + ModelImageExt);
        }

        private byte[] DownloadModelImage(string modelName)
        {
            var path = GetFileName(modelName);
            return null;// _downloader.Download(modelName, path);
        }

        public void LoadImage(string modelName, Action<bool, byte[]> callback)
        {
            DownloadModelImage(modelName, callback);
        }

        private void DownloadModelImage(string modelName, Action<bool, byte[]> callback)
        {
            var path = GetFileName(modelName);
            _downloader.Download(modelName, path, callback);
        }
    }
}
