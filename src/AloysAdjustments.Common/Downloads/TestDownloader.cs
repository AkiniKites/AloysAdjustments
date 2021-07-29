using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AloysAdjustments.Common.Downloads
{
    public class TestDownloader : ThrottledDownloaderOld<string>
    {
        private readonly WebClient _client;

        public TestDownloader(int maxStackSize)
            : base(maxStackSize)
        {
            _client = new WebClient();
        }

        protected override byte[] DownloadFile(string modelName)
        {
            if (false)
            {
                var dl = File.ReadAllLines(@"debug\urls.txt");
                var url = dl[new Random((int)DateTime.Now.Ticks).Next(dl.Length)];
                var bytes = _client.DownloadData(url);
                return ApplyText(bytes, modelName);
            }
            else
            {
                Thread.Sleep(1000);

                var files = Directory.GetFiles("debug\\img").ToList();
                var baseImg = files[new Random((int)DateTime.Now.Ticks).Next(files.Count)];

                var bytes = File.ReadAllBytes(baseImg);
                bytes = ApplyText(bytes, modelName);

                return bytes;
            }
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
                var textPosition = new Point(bmp.Height / 30, (bmp.Height / 30) * 29);
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
