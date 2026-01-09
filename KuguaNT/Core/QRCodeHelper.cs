using System.Drawing;
using System.Drawing.Imaging;
using ZXing;

namespace Kugua.Core
{
    public class QRCodeHelper
    {
        /// <summary>
        /// 生成二维码并返回 data:image/png;base64,... 格式的 Base64 字符串
        /// </summary>
        public static string GenerateQRCodeBase64(string text, int width = 300, int height = 300)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 4,
                    PureBarcode = false
                }
            };

            var pixelData = writer.Write(text);

            // 把 PixelData 转成 Bitmap，再转成 Base64
            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
            using (var ms = new MemoryStream())
            {
                var bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppRgb);

                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                bitmap.Save(ms, ImageFormat.Png);
                byte[] bytes = ms.ToArray();

                return Convert.ToBase64String(bytes);
            }
        }
    }
}
