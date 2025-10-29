using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using Image = System.Drawing.Image;

namespace Kugua.Core
{
    /// <summary>
    /// 计算图片相似度。基于区域哈希算法
    /// </summary>
    class ImageSimilar
    {
        Image SourceImg;

        //public SimilarPhoto(string filePath)
        //{
        //    SourceImg = Image.FromFile(filePath);
        //}

        public ImageSimilar(string base64)
        {
            if (!string.IsNullOrWhiteSpace(base64))
            {
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
                {
                    SourceImg = Image.FromStream(ms);
                }
            }
        }

        public ImageSimilar(Stream stream)
        {
            SourceImg = Image.FromStream(stream);
        }

        /// <summary>
        /// 计算图片的像素哈希
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        public static string GetHashFromBase64(string base64)
        {
            var img = new ImageSimilar(base64);
            return img.GetHash(base64);
        }

        public string GetHash(string base64)
        {
            var resized = ReduceSize();
            var blurred = ApplyGaussianBlur(resized);
            var grayValues = ReduceColor(blurred);
            var median = CalcMedian(grayValues);
            return ComputeBits(grayValues, median);
        }

        /// <summary>
        /// Step 1 : Reduce size to 8*8
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Image ReduceSize(int width = 8, int height = 8)
        {
            var destImage = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(SourceImg, 0, 0, width, height);
            }
            return destImage;
        }

        /// <summary>
        /// （高斯模糊）​减少噪声干扰，提升特征稳定性。
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private Bitmap ApplyGaussianBlur(Image image)
        {
            var blurred = new Bitmap(image.Width, image.Height);
            using (var graphics = Graphics.FromImage(blurred))
            using (var blurEffect = new ImageAttributes())
            {
                // 使用高斯模糊（需System.Drawing.Common）
                blurEffect.SetColorMatrix(new ColorMatrix(new float[][]
                {
                    new float[] { 0.1f, 0.1f, 0.1f, 0, 0 },
                    new float[] { 0.1f, 0.2f, 0.1f, 0, 0 },
                    new float[] { 0.1f, 0.1f, 0.1f, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                }));
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                    0, 0, image.Width, image.Height, GraphicsUnit.Pixel, blurEffect);
            }
            return blurred;
        }

        /// <summary>
        /// Step 2 : Reduce Color
        /// </summary>
        /// <param name="oriImage"></param>
        /// <returns></returns>
        private byte[] ReduceColor(Image image)
        {

            Bitmap bitMap = new Bitmap(image);
            byte[] grayValues = new byte[image.Width * image.Height];

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    Color color = bitMap.GetPixel(x, y);
                    // 使用浮点系数计算
                    //byte grayValue = (byte)((color.R * 30 + color.G * 59 + color.B * 11) / 100);
                    byte grayValue = (byte)(color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
                    grayValues[x * image.Width + y] = grayValue;
                }
            return grayValues;

        }


        /// <summary>
        /// 计算归一化汉明距离（相似度的百分比，范围在0~1）
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double CalcSimilarDegree(string a, string b)
        {
            if (a.Length != b.Length) throw new ArgumentException();
            int differences = a.Zip(b, (c1, c2) => c1 != c2 ? 1 : 0).Sum();
            return 1.0 - (double)differences / a.Length;
        }

        /// <summary>
        /// 计算中位数.平均值对光照敏感，改用中位数提升鲁棒性
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private byte CalcMedian(byte[] values)
        {
            var sorted = values.OrderBy(v => v).ToArray();
            int mid = sorted.Length / 2;
            return sorted.Length % 2 != 0 ? sorted[mid] : (byte)((sorted[mid - 1] + sorted[mid]) / 2);
        }

        private string ComputeBits(byte[] values, byte threshold, int width = 8)
        {
            var hash = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                // 差异哈希（比较右侧像素）
                if (i % width != width - 1)
                    hash.Append(values[i] > values[i + 1] ? "1" : "0");
            }
            return hash.ToString();
        }



    }
}
