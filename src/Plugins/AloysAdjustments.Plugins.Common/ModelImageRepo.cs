using AloysAdjustments.Logic;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AloysAdjustments.Common.Utility;

namespace AloysAdjustments.Plugins.Common
{
    public class ModelImageRepo
    {
        public const string ModelImageExt = ".jpg";
        
        private static readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _cacheLocks
            = new ConcurrentDictionary<string, ReaderWriterLockSlim>();

        public async Task<byte[]> LoadImage(string modelName)
        {
            var filename = GetFileName(modelName);
            if (!File.Exists(filename))
                return await DownloadModelImage(modelName);
            if (File.Exists(filename))
                return await ReadAllBytesAsync(filename);
            return new byte[0];
        }

        private string GetFileName(string modelName)
        {
            return Path.Combine(IoC.Config.ImageCachePath, modelName + ModelImageExt);
        }

        private async Task<byte[]> ReadAllBytesAsync(string path)
        {
            var cacheLock = _cacheLocks.GetOrAdd(path, x => new ReaderWriterLockSlim());

            using (cacheLock.UsingReaderLock())
            using (FileStream stream = File.Open(path, FileMode.Open))
            {
                var result = new byte[stream.Length];
                await stream.ReadAsync(result, 0, (int)stream.Length);
                return result;
            }
        }

        private async Task WriteAllBytesAsync(string path, byte[] bytes)
        {
            var cacheLock = _cacheLocks.GetOrAdd(path, x => new ReaderWriterLockSlim());

            using (cacheLock.UsingWriterLock())
            using (FileStream sourceStream = new FileStream(path,
                FileMode.Append, FileAccess.Write, FileShare.None,
                4096, true))
            {
                await sourceStream.WriteAsync(bytes, 0, bytes.Length);
            };
        }

        private async Task<byte[]> DownloadModelImage(string modelName)
        {
            var bytes = await FakeDownload(modelName);
            await WriteAllBytesAsync(GetFileName(modelName), bytes);
            
            return bytes;
        }

        private async Task<byte[]> FakeDownload(string modelName)
        {
            await Task.Delay(1000);

            var files = Directory.GetFiles(Path.Combine(IoC.Config.CachePath, "imgDebug")).ToList();
            var baseImg = files[new Random((int)DateTime.Now.Ticks).Next(files.Count)];
            
            var bytes = await ReadAllBytesAsync(baseImg);
            bytes = ApplyText(bytes, modelName);

            return bytes;
        }

        private byte[] ApplyText(byte[] img, string modelName)
        {
            using (var ms = new MemoryStream(img)) {
                var bmp = Image.FromStream(ms);
                var g = Graphics.FromImage(bmp);

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var fSize = bmp.Height / 30;
                var f = GetAdjustedFont(g, modelName, new Font("Arial", fSize), fSize);
                var textPosition = new Point(bmp.Height / 30, bmp.Height / 30);
                var size = g.MeasureString(modelName, f);
                var rect = new RectangleF(textPosition.X, textPosition.Y, size.Width, size.Height);
                g.FillRectangle(Brushes.Black, rect);
                g.DrawString(modelName, f, Brushes.LightGray, textPosition);

                g.Flush();

                using (var msOut = new MemoryStream())
                {
                    bmp.Save(msOut, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return msOut.ToArray();
                }
            }
        }

        public Font GetAdjustedFont(Graphics g, string graphicString, Font originalFont, int containerHeight)
        {
            // We utilize MeasureString which we get via a control instance           
            for (int adjustedSize = 100; adjustedSize >= 1; adjustedSize--)
            {
                var testFont = new Font(originalFont.Name, adjustedSize, originalFont.Style);

                // Test the string with the new size
                var adjustedSizeNew = g.MeasureString(graphicString, testFont);

                if (containerHeight > Convert.ToInt32(adjustedSizeNew.Height))
                    return testFont;
                testFont.Dispose();
            }
            
            return new Font(originalFont.Name, 1, originalFont.Style);
        }
    }
}
