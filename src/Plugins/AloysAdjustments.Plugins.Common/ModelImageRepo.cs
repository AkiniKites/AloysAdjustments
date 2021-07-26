﻿using AloysAdjustments.Logic;
using System;
using System.IO;
using AloysAdjustments.Common.Downloads;

namespace AloysAdjustments.Plugins.Common
{
    public class ModelImageRepo
    {
        public const string ModelImageExt = ".jpg";
        
        private readonly TestDownloader _downloader = new TestDownloader(10);

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
