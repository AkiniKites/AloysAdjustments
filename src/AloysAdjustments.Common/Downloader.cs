using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AloysAdjustments.Common.Utility;

namespace AloysAdjustments.Common
{
    public class Downloader
    {
        private readonly LimitedConcurrentStack<Action> _requests;

        private static readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _cacheLocks
            = new ConcurrentDictionary<string, ReaderWriterLockSlim>(StringComparer.OrdinalIgnoreCase);

        public Downloader()
        {
            _requests = new LimitedConcurrentStack<Action>(10);

            Task.Run(DownloadWorker);
        }

        public void Shutdown()
        {
            _requests.CompleteAdding();
        }

        public void Download(string url, string filePath, Action<bool, byte[]> callback)
        {
            var path = Path.GetFullPath(filePath);
            var cacheLock = _cacheLocks.GetOrAdd(path, x => new ReaderWriterLockSlim());
            if (GetExisting(path, cacheLock, callback))
                return;

            _requests.Push(() =>
            {
                try
                {
                    if (GetExisting(path, cacheLock, callback))
                        return;

                    Console.WriteLine("Miss: " + Path.GetFileName(filePath));
                    var bytes = FakeDownload(url, filePath);
                    using (cacheLock.UsingWriterLock())
                        File.WriteAllBytes(filePath, bytes);
                    callback(true, bytes);
                }
                catch (Exception)
                {
                    callback(false, null);
                }
            });
        }

        private bool GetExisting(string filePath, ReaderWriterLockSlim cacheLock, Action<bool, byte[]> callback)
        {
            using (cacheLock.UsingReaderLock())
            {
                if (File.Exists(filePath))
                {
                    Console.WriteLine("Hit: " + Path.GetFileName(filePath));
                    var bytes = File.ReadAllBytes(filePath);
                    callback(true, bytes);
                    return true;
                }
            }

            return false;
        }

        private void DownloadWorker()
        {
            while (_requests.TryPopWait(out var request))
            {
                request();
            }
        }


        private byte[] FakeDownload(string modelName, string path)
        {
            Thread.Sleep(1000);

            var files = Directory.GetFiles(Path.Combine("caches", "imgDebug")).ToList();
            var baseImg = files[new Random((int)DateTime.Now.Ticks).Next(files.Count)];

            var bytes = File.ReadAllBytes(baseImg);
            bytes = ApplyText(bytes, modelName);

            File.WriteAllBytes(path, bytes);

            return bytes;
        }

        private byte[] ApplyText(byte[] img, string modelName)
        {
            using (var ms = new MemoryStream(img))
            {
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
